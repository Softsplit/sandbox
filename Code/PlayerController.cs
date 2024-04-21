using Sandbox;
using Sandbox.Citizen;

public sealed class PlayerController : Component
{
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public float CrouchMoveSpeed { get; set; } = 60.0f;
	[Property] public float WalkMoveSpeed { get; set; } = 100.0f;
	[Property] public float RunMoveSpeed { get; set; } = 200.0f;
	[Property] public float SprintMoveSpeed { get; set; } = 400.0f;
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }

	[Sync] public bool ThirdPersonCamera { get; set; }
	[Sync] public bool Crouching { get; set; }
	[Sync] public bool Noclipping { get; set; }
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }

	public bool WishCrouch;
	public float EyeHeight = 64;

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			MouseInput();
		}

		UpdateAnimation();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( Input.Pressed( "noclip" ) )
		{
			Noclipping = !Noclipping;
		}

		if ( Noclipping )
		{
			NoclippingInput();
		}
		else
		{
			CrouchingInput();
			MovementInput();
		}
	}

	private void MouseInput()
	{
		var e = EyeAngles;
		e += Input.AnalogLook;
		e.pitch = e.pitch.Clamp( -90, 90 );
		e.roll = 0.0f;
		EyeAngles = e;
	}

	float CurrentMoveSpeed
	{
		get
		{
			if ( Crouching ) return CrouchMoveSpeed;
			if ( Input.Down( "run" ) ) return SprintMoveSpeed;
			if ( Input.Down( "walk" ) ) return WalkMoveSpeed;

			return RunMoveSpeed;
		}
	}

	RealTimeSince lastGrounded;
	RealTimeSince lastUngrounded;
	RealTimeSince lastJump;

	float GetFriction()
	{
		if ( CharacterController.IsOnGround ) return 6.0f;

		// air friction
		return 0.2f;
	}

	private void NoclippingInput()
	{
		var cc = CharacterController;

		var fwd = Input.AnalogMove.x.Clamp( -1f, 1f );
		var left = Input.AnalogMove.y.Clamp( -1f, 1f );
		var rotation = EyeAngles.ToRotation();

		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( "jump" ) )
		{
			vel += Vector3.Up * 1;
		}

		vel = vel.Normal * 2000;

		if ( Input.Down( "run" ) )
			vel *= 5.0f;

		if ( Input.Down( "duck" ) )
			vel *= 0.2f;

		cc.Velocity += vel * Time.Delta;

		if ( cc.Velocity.LengthSquared > 0.01f )
		{
			Transform.Position += cc.Velocity * Time.Delta;
		}

		cc.Velocity = cc.Velocity.Approach( 0, cc.Velocity.Length * Time.Delta * 5.0f );

		EyeAngles = rotation;
		WishVelocity = cc.Velocity;
	}

	private void MovementInput()
	{
		if ( CharacterController is null )
			return;

		var cc = CharacterController;

		Vector3 halfGravity = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

		WishVelocity = Input.AnalogMove;

		if ( lastGrounded < 0.2f && lastJump > 0.3f && Input.Pressed( "jump" ) )
		{
			lastJump = 0;
			cc.Punch( Vector3.Up * 300 );
		}

		if ( !WishVelocity.IsNearlyZero() )
		{
			WishVelocity = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation() * WishVelocity;
			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.ClampLength( 1 );
			WishVelocity *= CurrentMoveSpeed;

			if ( !cc.IsOnGround )
			{
				WishVelocity = WishVelocity.ClampLength( 50 );
			}
		}

		cc.ApplyFriction( GetFriction() );

		if ( cc.IsOnGround )
		{
			cc.Accelerate( WishVelocity );
			cc.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
		else
		{
			cc.Velocity += halfGravity;
			cc.Accelerate( WishVelocity );
		}

		//
		// Don't walk through other players, let them push you out of the way
		//
		var pushVelocity = PlayerPusher.GetPushVector( Transform.Position + Vector3.Up * 40.0f, Scene, GameObject );
		if ( !pushVelocity.IsNearlyZero() )
		{
			var travelDot = cc.Velocity.Dot( pushVelocity.Normal );
			if ( travelDot < 0 )
			{
				cc.Velocity -= pushVelocity.Normal * travelDot * 0.6f;
			}

			cc.Velocity += pushVelocity * 128.0f;
		}

		cc.Move();

		if ( !cc.IsOnGround )
		{
			cc.Velocity += halfGravity;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}

		if ( cc.IsOnGround )
		{
			lastGrounded = 0;
		}
		else
		{
			lastUngrounded = 0;
		}
	}
	float DuckHeight = (64 - 36);

	bool CanUncrouch()
	{
		if ( !Crouching ) return true;
		if ( lastUngrounded < 0.2f ) return false;

		var tr = CharacterController.TraceDirection( Vector3.Up * DuckHeight );
		return !tr.Hit; // hit nothing - we can!
	}

	public void CrouchingInput()
	{
		WishCrouch = Input.Down( "duck" );

		if ( WishCrouch == Crouching )
			return;

		// crouch
		if ( WishCrouch )
		{
			CharacterController.Height = 36;
			Crouching = WishCrouch;

			// if we're not on the ground, slide up our bbox so when we crouch
			// the bottom shrinks, instead of the top, which will mean we can reach
			// places by crouch jumping that we couldn't.
			if ( !CharacterController.IsOnGround )
			{
				CharacterController.MoveTo( Transform.Position += Vector3.Up * DuckHeight, false );
				Transform.ClearLerp();
				EyeHeight -= DuckHeight;
			}

			return;
		}

		// uncrouch
		if ( !WishCrouch )
		{
			if ( !CanUncrouch() ) return;

			CharacterController.Height = 64;
			Crouching = WishCrouch;
			return;
		}
	}

	private void UpdateCamera()
	{
		var camera = Components.GetInChildrenOrSelf<CameraComponent>();
		if ( camera is null ) return;

		var targetEyeHeight = Crouching ? 28 : 64;
		EyeHeight = EyeHeight.LerpTo( targetEyeHeight, RealTime.Delta * 10.0f );

		camera.Transform.Position = Transform.Position + new Vector3( 0, 0, EyeHeight );
		camera.Transform.Rotation = EyeAngles;
		camera.FieldOfView = Preferences.FieldOfView;

		if ( Input.Pressed( "view" ) )
		{
			ThirdPersonCamera = !ThirdPersonCamera;
		}

		if ( ThirdPersonCamera )
		{
			Vector3 targetPos;
			var center = Transform.Position + Vector3.Up * 64;

			var pos = center;
			var rot = camera.Transform.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			Vector3 distance = 130.0f * Transform.Scale;
			targetPos = pos + rot.Right * ((Components.GetInChildrenOrSelf<SkinnedModelRenderer>().Model.Bounds.Mins.x + 32) * Transform.Scale);
			targetPos += rot.Forward * -distance;

			var tr = Scene.Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.IgnoreGameObject( GameObject )
				.Radius( 8 )
				.Run();

			camera.Transform.Position = tr.EndPosition;
		}
	}

	protected override void OnPreRender()
	{
		UpdateBodyVisibility();

		if ( IsProxy )
			return;

		UpdateCamera();
	}

	private void UpdateAnimation()
	{
		if ( AnimationHelper is null ) return;

		// where should we be rotated to
		var turnSpeed = 0.02f;

		var idealRotation = Rotation.LookAt( EyeAngles.Forward.WithZ( 0 ), Vector3.Up );
		Transform.Rotation = Rotation.Slerp( Transform.Rotation, idealRotation, WishVelocity.Length * Time.Delta * turnSpeed );
		Transform.Rotation = Transform.Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		AnimationHelper.WithWishVelocity( WishVelocity );
		AnimationHelper.WithVelocity( CharacterController.Velocity );
		AnimationHelper.WithLook( EyeAngles.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		AnimationHelper.AimAngle = Transform.Rotation;
		AnimationHelper.FootShuffle = shuffle;
		AnimationHelper.DuckLevel = MathX.Lerp( AnimationHelper.DuckLevel, Crouching ? 1 : 0, Time.Delta * 10.0f );
		AnimationHelper.VoiceLevel = Components.GetInChildrenOrSelf<Voice>().LastPlayed < 0.5f ? Components.GetInChildrenOrSelf<Voice>().Amplitude : 0.0f;
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.IsNoclipping = Noclipping;
		AnimationHelper.IsWeaponLowered = false;
		AnimationHelper.MoveStyle = Input.Down( "run" ) ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
	}

	private void UpdateBodyVisibility()
	{
		if ( AnimationHelper is null )
			return;

		var renderMode = ModelRenderer.ShadowRenderType.On;
		if ( !IsProxy && !ThirdPersonCamera ) renderMode = ModelRenderer.ShadowRenderType.ShadowsOnly;

		AnimationHelper.Target.RenderType = renderMode;

		foreach ( var clothing in AnimationHelper.Target.Components.GetAll<ModelRenderer>( FindMode.InChildren ) )
		{
			if ( !clothing.Tags.Has( "clothing" ) )
				continue;

			clothing.RenderType = renderMode;
		}
	}

}
