using Sandbox.Citizen;

/// <summary>
/// Responsible for taking inputs from the player and moving them.
/// </summary>
public sealed class PlayerController : Component
{
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public float CrouchMoveSpeed { get; set; } = 64.0f;
	[Property] public float WalkMoveSpeed { get; set; } = 190.0f;
	[Property] public float RunMoveSpeed { get; set; } = 190.0f;
	[Property] public float SprintMoveSpeed { get; set; } = 320.0f;
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }

	[Sync] public bool Crouching { get; set; }
	[Sync] public Angles EyeAngles { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }

	public Vector3 EyePosition => WorldPosition + Vector3.Up * eyeHeight;

	public bool wishCrouch;
	public float eyeHeight = 64f;
	public float duckHeight = 28f;

	protected override void OnUpdate()
	{
		if ( !IsProxy )
		{
			MouseInput();
			WorldRotation = new Angles( 0f, EyeAngles.yaw, 0f );
		}

		UpdateAnimation();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		CrouchingInput();
		MovementInput();
	}

	private void MouseInput()
	{
		var player = Components.Get<Player>();
		var input = Input.AnalogLook;

		// allow listeners to modify the input eye angles
		Scene.RunEvent<IPlayerEvent>( x => x.OnCameraMove( player, ref input ) );

		var e = EyeAngles;
		e += input;
		e.pitch = e.pitch.Clamp( -90f, 90f );
		e.roll = 0f;
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
	float fallDistance;

	float GetFriction()
	{
		if ( CharacterController.IsOnGround ) return 6.0f;

		// air friction
		return 0.2f;
	}

	private void MovementInput()
	{
		if ( !CharacterController.IsValid() )
			return;

		var cc = CharacterController;
		Vector3 startPosition = WorldPosition;
		Vector3 startVelocity = cc.Velocity;
		Vector3 halfGravity = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

		WishVelocity = Input.AnalogMove;

		if ( lastGrounded < 0.2f && lastJump > 0.3f && Input.Pressed( "jump" ) )
		{
			lastJump = 0f;
			cc.Punch( Vector3.Up * 300f );
			IPlayerEvent.Post( x => x.OnJump( Components.Get<Player>() ) );
		}

		if ( !WishVelocity.IsNearlyZero() )
		{
			WishVelocity = new Angles( 0f, EyeAngles.yaw, 0f ).ToRotation() * WishVelocity;
			WishVelocity = WishVelocity.WithZ( 0f );
			WishVelocity = WishVelocity.ClampLength( 1f );
			WishVelocity *= CurrentMoveSpeed;

			if ( !cc.IsOnGround )
			{
				WishVelocity = WishVelocity.ClampLength( 50f );
			}
		}

		cc.ApplyFriction( GetFriction() );

		if ( cc.IsOnGround )
		{
			cc.Accelerate( WishVelocity );
			cc.Velocity = CharacterController.Velocity.WithZ( 0f );
		}
		else
		{
			cc.Velocity += halfGravity;
			cc.Accelerate( WishVelocity );
		}

		//
		// Don't walk through other players, let them push you out of the way
		//
		var pushVelocity = PlayerPusher.GetPushVector( WorldPosition + Vector3.Up * 40.0f, Scene, GameObject );
		if ( !pushVelocity.IsNearlyZero() )
		{
			var travelDot = cc.Velocity.Dot( pushVelocity.Normal );
			if ( travelDot < 0f )
			{
				cc.Velocity -= pushVelocity.Normal * travelDot * 0.6f;
			}

			cc.Velocity += pushVelocity * 128.0f;
		}

		cc.Move();

		Vector3 delta = startPosition - WorldPosition;

		if ( !cc.IsOnGround )
		{
			cc.Velocity += halfGravity;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0f );
		}

		if ( cc.IsOnGround )
		{
			if ( fallDistance > 0f )
			{
				IPlayerEvent.Post( x => x.OnLand( Components.Get<Player>(), fallDistance, startVelocity ) );
				fallDistance = 0f;
			}

			lastGrounded = 0f;
		}
		else
		{
			lastUngrounded = 0f;
			fallDistance += MathF.Max( 0f, delta.z );
		}
	}

	bool CanUncrouch()
	{
		if ( !Crouching ) return true;
		if ( lastUngrounded < 0.2f ) return false;

		var tr = CharacterController.TraceDirection( Vector3.Up * duckHeight );
		return !tr.Hit; // hit nothing - we can!
	}

	public void CrouchingInput()
	{
		wishCrouch = Input.Down( "duck" );

		if ( wishCrouch == Crouching )
			return;

		// crouch
		if ( wishCrouch )
		{
			CharacterController.Height = 36f;
			Crouching = wishCrouch;

			// if we're not on the ground, slide up our bbox so when we crouch
			// the bottom shrinks, instead of the top, which will mean we can reach
			// places by crouch jumping that we couldn't.
			if ( !CharacterController.IsOnGround )
			{
				CharacterController.MoveTo( WorldPosition += Vector3.Up * duckHeight, false );
				Transform.ClearInterpolation();
				eyeHeight -= duckHeight;
			}

			return;
		}

		// uncrouch
		if ( !wishCrouch )
		{
			if ( !CanUncrouch() ) return;

			CharacterController.Height = 72f;
			Crouching = wishCrouch;
			return;
		}
	}

	private void UpdateCamera()
	{
		var camera = Scene.GetAllComponents<CameraComponent>().Where( x => x.IsMainCamera ).FirstOrDefault();
		if ( !camera.IsValid() ) return;

		var targetEyeHeight = Crouching ? 28f : 64f;
		eyeHeight = eyeHeight.LerpTo( targetEyeHeight, RealTime.Delta * 10.0f );

		var targetCameraPos = WorldPosition + new Vector3( 0f, 0f, eyeHeight );

		// smooth view z, so when going up and down stairs or ducking, it's smooth af
		if ( lastUngrounded > 0.1f )
		{
			targetCameraPos.z = camera.WorldPosition.z.LerpTo( targetCameraPos.z, RealTime.Delta * 25.0f );
		}

		camera.WorldPosition = targetCameraPos;
		camera.WorldRotation = EyeAngles;
		camera.FieldOfView = Screen.CreateVerticalFieldOfView( Preferences.FieldOfView );

		// allow hooks
		var player = Components.Get<Player>();
		IPlayerEvent.Post( x => x.OnCameraSetup( player, camera ) );
		IPlayerEvent.Post( x => x.OnCameraPostSetup( player, camera ) );
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
		if ( !AnimationHelper.IsValid() ) return;

		var wv = WishVelocity.Length;

		AnimationHelper.WithWishVelocity( WishVelocity );
		AnimationHelper.WithVelocity( CharacterController.Velocity );
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.DuckLevel = Crouching ? 1.0f : 0.0f;
		AnimationHelper.MoveStyle = wv < 160f ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;

		var lookDir = EyeAngles.ToRotation().Forward * 1024f;
		AnimationHelper.WithLook( lookDir, 1, 0.5f, 0.25f );
	}

	private void UpdateBodyVisibility()
	{
		if ( !AnimationHelper.IsValid() )
			return;

		var renderMode = ModelRenderer.ShadowRenderType.On;
		if ( !IsProxy ) renderMode = ModelRenderer.ShadowRenderType.ShadowsOnly;

		AnimationHelper.Target.RenderType = renderMode;

		foreach ( var clothing in AnimationHelper.Target.Components.GetAll<ModelRenderer>( FindMode.InChildren ) )
		{
			if ( !clothing.Tags.Has( "clothing" ) )
				continue;

			clothing.RenderType = renderMode;
		}
	}
}
