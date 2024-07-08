using Sandbox.Citizen;

/// <summary>
/// This is what you should derive your player from. This base exists in addon code
/// so we can take advantage of codegen for replication. The side effect is that we
/// can put stuff in here that we don't need to access from the engine - which gives
/// more transparency to our code.
/// </summary>
[Title( "Player" ), Icon( "emoji_people" )]
public sealed class Player : Component, Component.ICollisionListener, Component.IDamageable, Component.INetworkSpawn
{
	[Property] public float Health { get; set; } = 100f;
	[Property] LifeState LifeState { get; set; } = LifeState.Alive;

	[Sync] public bool ThirdPersonCamera { get; set; }
	[Sync] public Vector3 InputDirection { get; set; }
	[Sync] public Angles ViewAngles { get; set; }
	public Angles OriginalViewAngles { get; private set; }

	/// <summary>
	/// Player's inventory for entities that can be carried. See <see cref="BaseCarriable"/>.
	/// </summary>
	public Inventory Inventory { get; protected set; }

	[RequireComponent] public PlayerController Controller { get; set; }

	/// <summary>
	/// The clothing container is what dresses the citizen
	/// </summary>
	public ClothingContainer Clothing = new();

	private TimeSince timeSinceDied;

	public Tool ActiveChild { get; set; }

	/// <summary>
	/// Called every tick to simulate the player. This is called on the
	/// client as well as the server (for prediction). So be careful!
	/// </summary>
	protected override void OnFixedUpdate()
	{
		if ( LifeState == LifeState.Dead )
		{
			if ( timeSinceDied > 3 )
			{
				Respawn();
			}

			return;
		}

		if ( LifeState != LifeState.Alive )
			return;

		UpdateBodyVisibility();
		UpdateAnimation();

		if ( Input.Pressed( "view" ) )
			ThirdPersonCamera = !ThirdPersonCamera;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		UpdatePlayerUse();

		OriginalViewAngles = ViewAngles;
		InputDirection = Input.AnalogMove;

		var look = Input.AnalogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
			look = look.WithYaw( look.yaw * -1f );

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;

		Scene.Camera.Transform.Rotation = ViewAngles;
		Scene.Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Preferences.FieldOfView );

		if ( ThirdPersonCamera )
		{
			Vector3 targetPos;
			var center = Transform.Position + Vector3.Up * 64;

			var pos = center;
			var rot = Scene.Camera.Transform.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			Vector3 distance = 130.0f * Transform.Scale * 0.6f;
			targetPos = pos + rot.Right * ((Components.GetInChildren<SkinnedModelRenderer>().Model.Bounds.Mins.x + 32) * Transform.Scale);
			targetPos += rot.Forward * -distance;

			var tr = Scene.Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Radius( 8 )
				.Run();

			Scene.Camera.Transform.Position = tr.EndPosition;
		}
		/*
		else if ( LifeState != LifeState.Alive && Corpse.IsValid() )
		{
			Corpse.EnableDrawing = true;

			var pos = Corpse.GetBoneTransform( 0 ).Position + Vector3.Up * 10;
			var targetPos = pos + Scene.Camera.Transform.Rotation.Backward * 100;

			var tr = Scene.Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Radius( 8 )
				.Run();

			Scene.Camera.Transform.Position = tr.EndPosition;
			Camera.FirstPersonViewer = null;
		}
		*/
		else
		{
			Scene.Camera.Transform.Position = EyePosition;
			// Camera.FirstPersonViewer = this;
			// Camera.Main.SetViewModelCamera( 90f );
		}
	}

	/// <summary>
	/// Applies flashbang-like ear ringing effect to the player.
	/// </summary>
	/// <param name="strength">Can be approximately treated as duration in seconds.</param>
	[Broadcast]
	public void Deafen( float strength )
	{
		// Sound.SetEffect( "flashbang", strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	public void OnNetworkSpawn( Connection owner )
	{
		Clothing.Deserialize( owner.GetUserData( "avatar" ) );
		Clothing.Apply( Components.GetInChildren<SkinnedModelRenderer>() );
	}

	protected override void OnStart()
	{
		Tags.Add( "player" );

		Inventory = new( this );

		Components.GetInChildren<SkinnedModelRenderer>().OnFootstepEvent += OnAnimEventFootstep;

		Respawn();
	}

	/// <summary>
	/// Called once the player's health reaches 0.
	/// </summary>
	public void OnKilled()
	{
		GameManager.Current?.OnKilled( GameObject );

		Components.GetInChildren<SkinnedModelRenderer>().OnFootstepEvent -= OnAnimEventFootstep;

		timeSinceDied = 0;
		LifeState = LifeState.Dead;
		StopUsing();

		// Client?.AddInt( "deaths", 1 );
	}

	[ConCmd( "kill" )]
	public static void KillSelf()
	{
		var owner = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();

		if ( owner == null )
			return;

		owner.OnKilled();
	}

	/// <summary>
	/// Sets LifeState to Alive, Health to Max, nulls velocity, and calls Gamemode.PlayerRespawn
	/// </summary>
	public void Respawn()
	{
		// var physgun = new GameObject().Components.Create<Physgun>();
		// Inventory.Add( physgun );

		Components.GetInChildren<SkinnedModelRenderer>().OnFootstepEvent += OnAnimEventFootstep;

		LifeState = LifeState.Alive;
		Health = 100;
		Controller.Velocity = Vector3.Zero;

		GameManager.Current?.MoveToSpawnpoint( GameObject );
		Transform.ClearInterpolation();
	}

	/// <summary>
	/// A generic corpse entity
	/// </summary>
	public GameObject Corpse { get; set; }

	private TimeSince timeSinceLastFootstep = 0;

	/// <summary>
	/// A footstep has arrived!
	/// </summary>
	public void OnAnimEventFootstep( SceneModel.FootstepEvent e )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		e.Volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Scene.Trace.Ray( e.Transform.Position, e.Transform.Position + Vector3.Down * 20 )
			.Radius( 1 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( GameObject, tr, e.FootId, e.Volume );
	}

	/// <summary>
	/// Allows override of footstep sound volume.
	/// </summary>
	/// <returns>The new footstep volume, where 1 is full volume.</returns>
	public float FootstepVolume()
	{
		return Controller.Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
	}

	void UpdateAnimation()
	{
		// where should we be rotated to
		var turnSpeed = 0.02f;
		var idealRotation = Rotation.LookAt( ViewAngles.Forward.WithZ( 0 ), Vector3.Up );
		Transform.Rotation = Rotation.Slerp( Transform.Rotation, idealRotation, Controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Transform.Rotation = Transform.Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		var animationHelper = Components.GetInChildren<CitizenAnimationHelper>();
		animationHelper.WithWishVelocity( Controller.WishVelocity );
		animationHelper.WithVelocity( Controller.Velocity );
		animationHelper.WithLook( EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animationHelper.AimAngle = ViewAngles;
		animationHelper.MoveRotationSpeed = shuffle;
		animationHelper.DuckLevel = MathX.Lerp( animationHelper.DuckLevel, Controller.Tags.Has( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animationHelper.IsGrounded = Controller.GroundObject != null;
		animationHelper.IsSitting = Controller.Tags.Has( "sitting" );
		animationHelper.IsNoclipping = Controller.Tags.Has( "noclip" );
		animationHelper.IsClimbing = Controller.Tags.Has( "climbing" );
		animationHelper.IsWeaponLowered = false;
		animationHelper.MoveStyle = Input.Down( "run" ) ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
	}

	void UpdateBodyVisibility()
	{
		var renderMode = ModelRenderer.ShadowRenderType.On;
		var animationHelper = Components.GetInChildren<CitizenAnimationHelper>();
		if ( !IsProxy && !ThirdPersonCamera ) renderMode = ModelRenderer.ShadowRenderType.ShadowsOnly;

		animationHelper.Target.RenderType = renderMode;

		foreach ( var clothing in animationHelper.Target.Components.GetAll<ModelRenderer>( FindMode.InChildren ) )
		{
			if ( !clothing.Tags.Has( "clothing" ) )
				continue;

			clothing.RenderType = renderMode;
		}
	}

	public void OnCollisionStart( Collision other )
	{
		if ( !IsProxy )
			return;

		/*
		if ( other is PickupTrigger )
		{
			StartTouch( other.Parent );
			return;
		}

		Inventory?.Add( other, Inventory.Active == null );
		*/
	}

	public void OnDamage( in DamageInfo damage )
	{
		if ( LifeState == LifeState.Dead )
			return;

		if ( Health > 0f && LifeState == LifeState.Alive )
		{
			Health -= damage.Damage;
			if ( Health <= 0f )
			{
				Health = 0f;
				OnKilled();
			}
		}

		// this.ProceduralHitReaction( damage );

		//
		// Add a score to the killer
		//
		if ( LifeState == LifeState.Dead && damage.Attacker != null )
		{
			if ( damage.Attacker.Network.OwnerConnection != null && damage.Attacker != GameObject )
			{
				// damage.Attacker.Client.AddInt( "kills" );
			}
		}

		if ( damage.IsExplosion )
		{
			Deafen( damage.Damage.LerpInverse( 0, 60 ) );
		}
	}

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	public Vector3 EyePosition
	{
		get => Transform.Local.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.World.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Sync] public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	public Rotation EyeRotation
	{
		get => Transform.Local.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.World.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Sync]
	public Rotation EyeLocalRotation { get; set; }

	/// <summary>
	/// Override the aim ray to use the player's eye position and rotation.
	/// </summary>
	public Ray AimRay => new( EyePosition, EyeRotation.Forward );

	/// <summary>
	/// Entity the player is currently using via their interaction key.
	/// </summary>
	public GameObject Using { get; set; }

	/// <summary>
	/// This should be called somewhere in your player's tick to allow them to use entities
	/// </summary>
	void UpdatePlayerUse()
	{
		if ( Input.Pressed( "use" ) )
		{
			Using = FindUsable();

			if ( Using == null )
				return;
		}

		if ( !Input.Down( "use" ) )
		{
			StopUsing();
			return;
		}

		if ( !Using.IsValid() )
			return;

		// If we move too far away or something we should probably ClearUse()?

		//
		// If use returns true then we can keep using it
		//
		if ( Using.Components.GetAll().FirstOrDefault() is IUse use && use.OnUse( GameObject ) )
			return;

		StopUsing();
	}

	/// <summary>
	/// If we're using an entity, stop using it
	/// </summary>
	void StopUsing()
	{
		Using = null;
	}

	/// <summary>
	/// Returns if the entity is a valid usable entity
	/// </summary>
	bool IsValidUseEntity( GameObject e )
	{
		if ( e == null ) return false;
		if ( e.Components.GetAll().FirstOrDefault() is not IUse use ) return false;
		if ( !use.IsUsable( GameObject ) ) return false;

		return true;
	}

	/// <summary>
	/// Find a usable entity for this player to use
	/// </summary>
	GameObject FindUsable()
	{
		// First try a direct 0 width line
		var tr = Scene.Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 85 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		// See if any of the parent entities are usable if we ain't.
		var ent = tr.GameObject;
		while ( ent.IsValid() && !IsValidUseEntity( ent ) )
		{
			ent = ent.Parent;
		}

		// Nothing found, try a wider search
		if ( !IsValidUseEntity( ent ) )
		{
			tr = Scene.Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 85 )
			.Radius( 2 )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

			// See if any of the parent entities are usable if we ain't.
			ent = tr.GameObject;
			while ( ent.IsValid() && !IsValidUseEntity( ent ) )
			{
				ent = ent.Parent;
			}
		}

		// Still no good? Bail.
		if ( !IsValidUseEntity( ent ) ) return null;

		return ent;
	}
}
