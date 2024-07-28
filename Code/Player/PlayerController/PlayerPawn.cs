namespace Softsplit;

public sealed partial class PlayerPawn : Pawn, IDescription
{
	/// <summary>
	/// The player's body
	/// </summary>
	[Property] public bool lockCamera { get; set; }
	[Property] public PlayerBody Body { get; set; }

	/// <summary>
	/// A reference to the player's head (the GameObject)
	/// </summary>
	[Property] public GameObject Head { get; set; }

	/// <summary>
	/// A reference to the animation helper (normally on the Body GameObject)
	/// </summary>
	[Property] public AnimationHelper AnimationHelper { get; set; }

	/// <summary>
	/// The current character controller for this player.
	/// </summary>
	[RequireComponent] public CharacterController CharacterController { get; set; }

	/// <summary>
	/// The current camera controller for this player.
	/// </summary>
	[RequireComponent] public CameraController CameraController { get; set; }

	/// <summary>
	/// A reference to the View Model's camera. This will be disabled by the View Model.
	/// </summary>
	[Property] public CameraComponent ViewModelCamera { get; set; }

	/// <summary>
	/// Handles the player's outfit
	/// </summary>
	[Property] public PlayerOutfitter Outfitter { get; set; }

	/// <summary>
	/// Get a quick reference to the real Camera GameObject.
	/// </summary>
	public GameObject CameraGameObject => CameraController.Camera.GameObject;

	/// <summary>
	/// Finds the first <see cref="SkinnedModelRenderer"/> on <see cref="Body"/>
	/// </summary>
	public SkinnedModelRenderer BodyRenderer => Body?.Components?.Get<SkinnedModelRenderer>();

	// IDescription
	string IDescription.DisplayName => DisplayName;

	/// <summary>
	/// Pawn
	/// </summary>
	// CameraComponent Pawn.Camera => CameraController.Camera;

	public override CameraComponent Camera => CameraController.Camera;

	public override string NameType => "Player";

	protected override void OnStart()
	{
		// TODO: expose these parameters please
		TagBinder.BindTag( "no_shooting", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceWeaponDeployed < 0.66f );
		TagBinder.BindTag( "no_aiming", () => IsSprinting || TimeSinceSprintChanged < 0.25f || TimeSinceGroundedChanged < 0.25f );

		GameObject.Name = $"Player ({DisplayName})";

		CameraController.SetActive( IsViewer );

		
	}

	public SceneTraceResult CachedEyeTrace { get; private set; }

	public static string GetInputKey()
	{
		foreach(InputAction iA in Input.GetActions())
		{
			if(Input.Pressed(iA.Name)) return iA.Name;
			
		}
		return null;
	}

	protected override void OnUpdate()
	{
		if ( HealthComponent.State == LifeState.Dead )
		{
			UpdateDead();
		}

		OnUpdateMovement();

		CrouchAmount = CrouchAmount.LerpTo( IsCrouching ? 1 : 0, Time.Delta * Global.CrouchLerpSpeed );
		_smoothEyeHeight = _smoothEyeHeight.LerpTo( _eyeHeightOffset * (IsCrouching ? CrouchAmount : 1), Time.Delta * 10f );
		CharacterController.Height = Height + _smoothEyeHeight;

		if ( IsLocallyControlled )
		{
			DebugUpdate();
		}
	}

	private float DeathcamSkipTime => 5f;
	private float DeathcamIgnoreInputTime => 1f;

	// deathcam
	private void UpdateDead()
	{
		if ( !IsLocallyControlled )
			return;

		if ( !PlayerState.IsValid() )
			return;

		if ( PlayerState.GetLastKiller() is { IsValid: true } killerPlayer )
		{
			EyeAngles = Rotation.LookAt( killerPlayer.Transform.Position - Transform.Position, Vector3.Up );
		}
		if ( ((Input.Pressed( "attack1" ) || Input.Pressed( "attack2" )) && !PlayerState.IsRespawning) || PlayerState.IsBot || PlayerState.LastDamageInfo.TimeSinceEvent > DeathcamSkipTime )
		{
			// Don't let players immediately switch
			if ( PlayerState.LastDamageInfo.TimeSinceEvent < DeathcamIgnoreInputTime ) return;

			GameObject.Destroy();
			return;
		}
	}

	protected override void OnFixedUpdate()
	{
		var cc = CharacterController;
		if ( !cc.IsValid() ) return;

		var wasGrounded = IsGrounded;
		IsGrounded = cc.IsOnGround;

		if ( IsGrounded != wasGrounded )
		{
			GroundedChanged( wasGrounded, IsGrounded );
		}

		UpdateEyes();

		if ( IsViewer )
		{
			CachedEyeTrace = Scene.Trace.Ray( AimRay, 100000f )
				.IgnoreGameObjectHierarchy( GameObject )
				.WithoutTags( "ragdoll", "movement" )
				.UseHitboxes()
				.Run();
		}

		if ( HealthComponent.State != LifeState.Alive )
		{
			return;
		}

		if ( Networking.IsHost && PlayerState.IsBot )
		{
			if ( BotFollowHostInput )
			{
				BuildWishInput();
				BuildWishVelocity();
			}

			// If we're a bot call these so they don't float in the air.
			ApplyAcceleration();
			ApplyMovement();
			return;
		}

		if ( !IsLocallyControlled )
		{
			return;
		}

		_previousVelocity = cc.Velocity;

		UpdateUse();
		BuildWishInput();
		BuildWishVelocity();
		BuildInput();

		UpdateRecoilAndSpread();
		ApplyAcceleration();
		ApplyMovement();
	}
}
