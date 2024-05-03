using Sandbox.Citizen;
using System;

public sealed class PlayerController : Component
{
	[ConVar( "debug_playercontroller" )] public static bool Debug { get; set; } = false;

	[Property, Sync] public float SprintSpeed { get; set; } = 320.0f;
	[Property, Sync] public float WalkSpeed { get; set; } = 150.0f;
	[Property, Sync] public float DefaultSpeed { get; set; } = 190.0f;
	[Property, Sync] public float Acceleration { get; set; } = 10.0f;
	[Property, Sync] public float AirAcceleration { get; set; } = 50.0f;
	[Property, Sync] public float FallSoundZ { get; set; } = -30.0f;
	[Property, Sync] public float GroundFriction { get; set; } = 4.0f;
	[Property, Sync] public float StopSpeed { get; set; } = 100.0f;
	[Property, Sync] public float Size { get; set; } = 20.0f;
	[Property, Sync] public float DistEpsilon { get; set; } = 0.03125f;
	[Property, Sync] public float GroundAngle { get; set; } = 46.0f;
	[Property, Sync] public float Bounce { get; set; } = 0.0f;
	[Property, Sync] public float MoveFriction { get; set; } = 1.0f;
	[Property, Sync] public float StepSize { get; set; } = 18.0f;
	[Property, Sync] public float MaxNonJumpVelocity { get; set; } = 140.0f;
	[Property, Sync] public float BodyGirth { get; set; } = 32.0f;
	[Property, Sync] public float BodyHeight { get; set; } = 72.0f;
	[Property, Sync] public float EyeHeight { get; set; } = 64.0f;
	[Property, Sync] public float Gravity { get; set; } = 800.0f;
	[Property, Sync] public float AirControl { get; set; } = 30.0f;
	[Property, Sync] public bool Swimming { get; set; } = false;
	[Property, Sync] public bool AutoJump { get; set; } = false;

	[Sync] public Vector3 BaseVelocity { get; set; }
	[Sync] public Vector3 Velocity { get; set; }
	[Sync] public Vector3 WishVelocity { get; set; }
	public GameObject GroundObject { get; set; }
	[Sync] public Vector3 GroundNormal { get; set; }

	[RequireComponent] public Duck Duck { get; set; }
	[RequireComponent] public Unstuck Unstuck { get; set; }

	// Duck body height 32
	// Eye Height 64
	// Duck Eye Height 28

	Vector3 mins;
	Vector3 maxs;

	public void SetBBox( Vector3 mins, Vector3 maxs )
	{
		if ( this.mins == mins && this.maxs == maxs )
			return;

		this.mins = mins;
		this.maxs = maxs;
	}

	/// <summary>
	/// Update the size of the bbox. We should really trigger some shit if this changes.
	/// </summary>
	public void UpdateBBox()
	{
		var girth = BodyGirth * 0.5f;
		var mins = new Vector3( -girth, -girth, 0 ) * Transform.Scale;
		var maxs = new Vector3( +girth, +girth, BodyHeight ) * Transform.Scale;

		Duck.UpdateBBox( ref mins, ref maxs );

		SetBBox( mins, maxs );
	}

	float SurfaceFriction;
	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		var pl = Components.Get<Player>();
		pl.EyeRotation = pl.ViewAngles.ToRotation();
	}

	[Sync] public bool IsNoclipping { get; set; } = false;

	protected override void OnFixedUpdate()
	{
		Tags.Set( "noclip", IsNoclipping );

		if ( Input.Pressed( "noclip" ) )
			IsNoclipping = !IsNoclipping;

		if ( IsNoclipping )
			NoclipUpdate();
		else
			MovementUpdate();
	}

	void NoclipUpdate()
	{
		var pl = Components.Get<Player>();

		var fwd = pl.InputDirection.x.Clamp( -1f, 1f );
		var left = pl.InputDirection.y.Clamp( -1f, 1f );
		var rotation = pl.ViewAngles.ToRotation();
		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( "jump" ) )
			vel += Vector3.Up * 1;

		vel = vel.Normal * 2000;

		if ( Input.Down( "run" ) )
			vel *= 5.0f;

		if ( Input.Down( "duck" ) )
			vel *= 0.2f;

		Velocity += vel * Time.Delta;

		if ( Velocity.LengthSquared > 0.01f )
			Transform.Position += Velocity * Time.Delta;

		Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );
	}

	void MovementUpdate()
	{
		var pl = Components.Get<Player>();

		pl.EyeLocalPosition = Vector3.Up * (EyeHeight * Transform.Scale);

		UpdateBBox();

		pl.EyeLocalPosition += TraceOffset;

		if ( Unstuck.TestAndFix() )
			return;

		// RunLadderMode
		CheckLadder();

		//
		// Start Gravity
		//
		if ( !Swimming && !IsTouchingLadder )
		{
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

			BaseVelocity = BaseVelocity.WithZ( 0 );
		}

		if ( AutoJump ? Input.Down( "jump" ) : Input.Pressed( "jump" ) )
			CheckJumpButton();

		// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor,
		//  we don't slow when standing still, relative to the conveyor.
		bool bStartOnGround = GroundObject != null;
		if ( bStartOnGround )
		{
			Velocity = Velocity.WithZ( 0 );

			if ( GroundObject != null )
			{
				ApplyFriction( GroundFriction * SurfaceFriction );
			}
		}

		//
		// Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
		//
		WishVelocity = new Vector3( pl.InputDirection.x.Clamp( -1f, 1f ), pl.InputDirection.y.Clamp( -1f, 1f ), 0 );

		var inSpeed = WishVelocity.Length.Clamp( 0, 1 );

		WishVelocity *= pl.ViewAngles.WithPitch( 0 ).ToRotation();

		if ( !Swimming && !IsTouchingLadder )
			WishVelocity = WishVelocity.WithZ( 0 );

		WishVelocity = WishVelocity.Normal * inSpeed;
		WishVelocity *= GetWishSpeed();

		Duck.PreTick();

		bool bStayOnGround = false;

		if ( Swimming )
		{
			ApplyFriction( 1 );
			WaterMove();
		}
		else if ( IsTouchingLadder )
		{
			LadderMove();
		}
		else if ( GroundObject != null )
		{
			bStayOnGround = true;
			WalkMove();
		}
		else
		{
			AirMove();
		}

		CategorizePosition( bStayOnGround );

		// FinishGravity
		if ( !Swimming && !IsTouchingLadder )
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		if ( GroundObject != null )
			Velocity = Velocity.WithZ( 0 );

		/*
		if ( Debug )
		{
			DebugOverlay.Box( Position + TraceOffset, mins, maxs, Color.Red );
			DebugOverlay.Box( Position, mins, maxs, Color.Blue );

			var lineOffset = 0;
			if ( Game.IsServer ) lineOffset = 10;

			DebugOverlay.ScreenText( $"        Position: {Position}", lineOffset + 0 );
			DebugOverlay.ScreenText( $"        Velocity: {Velocity}", lineOffset + 1 );
			DebugOverlay.ScreenText( $"    BaseVelocity: {BaseVelocity}", lineOffset + 2 );
			DebugOverlay.ScreenText( $"    GroundObject: {GroundObject} [{GroundObject?.Velocity}]", lineOffset + 3 );
			DebugOverlay.ScreenText( $" SurfaceFriction: {SurfaceFriction}", lineOffset + 4 );
			DebugOverlay.ScreenText( $"    WishVelocity: {WishVelocity}", lineOffset + 5 );
			DebugOverlay.ScreenText( $"    Speed: {Velocity.Length}", lineOffset + 6 );
		}
		*/
	}

	public float GetWishSpeed()
	{
		var ws = Duck.GetWishSpeed();
		if ( ws >= 0 ) return ws;

		if ( Input.Down( "run" ) ) return SprintSpeed;
		if ( Input.Down( "walk" ) ) return WalkSpeed;

		return DefaultSpeed;
	}

	public void WalkMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		WishVelocity = WishVelocity.WithZ( 0 );
		WishVelocity = WishVelocity.Normal * wishspeed;

		Velocity = Velocity.WithZ( 0 );

		Accelerate( wishdir, wishspeed, 0, Acceleration );

		Velocity = Velocity.WithZ( 0 );

		// Add in any base velocity to the current velocity.
		Velocity += BaseVelocity;

		try
		{
			if ( Velocity.Length < 1.0f )
			{
				Velocity = Vector3.Zero;
				return;
			}

			// first try just moving to the destination
			var dest = (Transform.Position + Velocity * Time.Delta).WithZ( Transform.Position.z );

			var pm = TraceBBox( Transform.Position, dest );

			if ( pm.Fraction == 1 )
			{
				Transform.Position = pm.EndPosition;
				StayOnGround();
				return;
			}

			StepMove();
		}
		finally
		{
			// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
			Velocity -= BaseVelocity;
		}

		StayOnGround();

		Velocity = Velocity.Normal * MathF.Min( Velocity.Length, GetWishSpeed() );
	}

	public void StepMove()
	{
		MoveHelper mover = new( Transform.Position, Velocity )
		{
			Trace = Scene.Trace.Size( mins, maxs ).IgnoreGameObjectHierarchy( GameObject ),
			MaxStandableAngle = GroundAngle
		};

		mover.TryMoveWithStep( Time.Delta, StepSize );

		Transform.Position = mover.Position;
		Velocity = mover.Velocity;
	}

	public void Move()
	{
		MoveHelper mover = new( Transform.Position, Velocity )
		{
			Trace = Scene.Trace.Size( mins, maxs ).IgnoreGameObjectHierarchy( GameObject ),
			MaxStandableAngle = GroundAngle
		};

		mover.TryMove( Time.Delta );

		Transform.Position = mover.Position;
		Velocity = mover.Velocity;
	}

	/// <summary>
	/// Add our wish direction and speed onto our velocity
	/// </summary>
	public void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		// See if we are changing direction a bit
		var currentspeed = Velocity.Dot( wishdir );

		// Reduce wishspeed by the amount of veer.
		var addspeed = wishspeed - currentspeed;

		// If not going to add any speed, done.
		if ( addspeed <= 0 )
			return;

		// Determine amount of acceleration.
		var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

		// Cap at addspeed
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		Velocity += wishdir * accelspeed;
	}

	/// <summary>
	/// Remove ground friction from velocity
	/// </summary>
	public void ApplyFriction( float frictionAmount = 1.0f )
	{
		// Calculate speed
		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		// Bleed off some speed, but if we have less than the bleed
		//  threshold, bleed the threshold amount.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}

	public void CheckJumpButton()
	{
		if ( IsProxy ) return;

		// If we are in the water most of the way...
		if ( Swimming )
		{
			// swimming, not jumping
			ClearGroundObject();

			Velocity = Velocity.WithZ( 100 );

			return;
		}

		if ( GroundObject == null )
			return;

		ClearGroundObject();

		float flGroundFactor = 1.0f;
		float flMul = 268.3281572999747f * 1.2f;
		float startz = Velocity.z;

		if ( Duck.IsActive )
			flMul *= 0.8f;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		Components.GetInChildren<CitizenAnimationHelper>().TriggerJump();
	}

	public void AirMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );

		Velocity += BaseVelocity;

		Move();

		Velocity -= BaseVelocity;
	}

	public void WaterMove()
	{
		var wishdir = WishVelocity.Normal;
		var wishspeed = WishVelocity.Length;

		wishspeed *= 0.8f;

		Accelerate( wishdir, wishspeed, 100, Acceleration );

		Velocity += BaseVelocity;

		Move();

		Velocity -= BaseVelocity;
	}

	bool IsTouchingLadder = false;
	Vector3 LadderNormal;

	public void CheckLadder()
	{
		Tags.Set( "climbing", IsTouchingLadder );

		var pl = Components.Get<Player>();

		var wishvel = new Vector3( pl.InputDirection.x.Clamp( -1f, 1f ), pl.InputDirection.y.Clamp( -1f, 1f ), 0 );
		wishvel *= pl.ViewAngles.WithPitch( 0 ).ToRotation();
		wishvel = wishvel.Normal;

		if ( IsTouchingLadder )
		{
			if ( Input.Pressed( "jump" ) )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;

				return;

			}
			else if ( GroundObject != null && LadderNormal.Dot( wishvel ) > 0 )
			{
				IsTouchingLadder = false;

				return;
			}
		}

		const float ladderDistance = 1.0f;
		var start = Transform.Position;
		Vector3 end = start + (IsTouchingLadder ? (LadderNormal * -1.0f) : wishvel) * ladderDistance;

		var pm = Scene.Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithTag( "ladder" )
					.IgnoreGameObjectHierarchy( GameObject )
					.Run();

		IsTouchingLadder = false;

		if ( pm.Hit )
		{
			IsTouchingLadder = true;
			LadderNormal = pm.Normal;
		}
	}

	public void LadderMove()
	{
		var velocity = WishVelocity;
		float normalDot = velocity.Dot( LadderNormal );
		var cross = LadderNormal * normalDot;

		Velocity = velocity - cross + (-normalDot * LadderNormal.Cross( Vector3.Up.Cross( LadderNormal ).Normal ));

		Move();
	}


	public void CategorizePosition( bool bStayOnGround )
	{
		SurfaceFriction = 1.0f;

		var point = Transform.Position - Vector3.Up * 2;
		var vBumpOrigin = Transform.Position;

		//
		//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
		//
		bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
		bool bMoveToEndPos = false;

		if ( GroundObject != null ) // and not underwater
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}
		else if ( bStayOnGround )
		{
			bMoveToEndPos = true;
			point.z -= StepSize;
		}

		if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
		{
			ClearGroundObject();
			return;
		}

		var pm = TraceBBox( vBumpOrigin, point, 4.0f );

		if ( pm.GameObject == null || Vector3.GetAngle( Vector3.Up, pm.Normal ) > GroundAngle )
		{
			ClearGroundObject();
			bMoveToEndPos = false;

			if ( Velocity.z > 0 )
				SurfaceFriction = 0.25f;
		}
		else
		{
			UpdateGroundObject( pm );
		}

		if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
		{
			Transform.Position = pm.EndPosition;
		}
	}

	/// <summary>
	/// We have a new ground entity
	/// </summary>
	public void UpdateGroundObject( SceneTraceResult tr )
	{
		GroundNormal = tr.Normal;

		// VALVE HACKHACK: Scale this to fudge the relationship between vphysics friction values and player friction values.
		// A value of 0.8f feels pretty normal for vphysics, whereas 1.0f is normal for players.
		// This scaling trivially makes them equivalent.  REVISIT if this affects low friction surfaces too much.
		SurfaceFriction = tr.Surface.Friction * 1.25f;
		if ( SurfaceFriction > 1 ) SurfaceFriction = 1;

		GroundObject = tr.GameObject;
	}

	/// <summary>
	/// We're no longer on the ground, remove it
	/// </summary>
	public void ClearGroundObject()
	{
		if ( GroundObject == null ) return;

		GroundObject = null;
		GroundNormal = Vector3.Up;
		SurfaceFriction = 1.0f;
	}

	/// <summary>
	/// Any bbox traces we do will be offset by this amount.
	/// todo: this needs to be predicted
	/// </summary>
	public Vector3 TraceOffset;

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public SceneTraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Scene.Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
					.IgnoreGameObjectHierarchy( GameObject )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	/// <summary>
	/// Traces the current bbox and returns the result.
	/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public SceneTraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, mins, maxs, liftFeet );
	}

	/// <summary>
	/// Try to keep a walking player on the ground when running down slopes etc
	/// </summary>
	public void StayOnGround()
	{
		var start = Transform.Position + Vector3.Up * 2;
		var end = Transform.Position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = TraceBBox( Transform.Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return;

		Transform.Position = trace.EndPosition;
	}
}
