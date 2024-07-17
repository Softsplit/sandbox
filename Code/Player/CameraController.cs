namespace Softsplit;

public enum CameraMode
{
	FirstPerson,
	ThirdPerson
}

public sealed class CameraController : Component
{
	/// <summary>
	/// A reference to the camera component we're going to be doing stuff with.
	/// </summary>
	[Property] public CameraComponent Camera { get; set; }
	[Property] public GameObject Boom { get; set; }
	[Property] public AudioListener AudioListener { get; set; }
	[Property] public PlayerPawn Player { get; set; }

	[Property] public float ThirdPersonDistance { get; set; } = 128f;
	[Property] public float AimFovOffset { get; set; } = -5f;

	private CameraMode _mode;

	public CameraMode Mode
	{
		get => _mode;
		set
		{
			_mode = value;
			OnModeChanged();
		}
	}

	public float MaxBoomLength { get; set; }

	/// <summary>
	/// Constructs a ray using the camera's GameObject
	/// </summary>
	public Ray AimRay => new( Camera.Transform.Position + Camera.Transform.Rotation.Forward, Camera.Transform.Rotation.Forward );

	private float FieldOfViewOffset = 0f;
	private float TargetFieldOfView = 90f;

	public void AddFieldOfViewOffset( float degrees )
	{
		FieldOfViewOffset -= degrees;
	}

	public void SetActive( bool isActive )
	{
		Camera.Enabled = isActive;
		AudioListener.Enabled = isActive;

		OnModeChanged();

		Boom.Transform.Rotation = Player.EyeAngles.ToRotation();
	}

	/// <summary>
	/// Updates the camera's position, from player code
	/// </summary>
	/// <param name="eyeHeight"></param>
	internal void UpdateFromEyes( float eyeHeight )
	{
		// All transform effects are additive to camera local position, so we need to reset it before anything is applied
		Camera.Transform.LocalPosition = Vector3.Zero;
		Camera.Transform.LocalRotation = Rotation.Identity;

		if ( Mode == CameraMode.ThirdPerson && !Player.IsLocallyControlled )
		{
			// orbit cam: spectating only
			var angles = Boom.Transform.Rotation.Angles();
			angles += Input.AnalogLook;
			Boom.Transform.Rotation = angles.WithPitch( angles.pitch.Clamp( -90, 90 ) ).ToRotation();
		}
		else
		{
			Boom.Transform.Rotation = Player.EyeAngles.ToRotation();
		}

		if ( MaxBoomLength > 0 )
		{
			var tr = Scene.Trace.Ray( new Ray( Boom.Transform.Position, Boom.Transform.Rotation.Backward ), MaxBoomLength )
				.IgnoreGameObjectHierarchy( GameObject.Root )
				.WithoutTags( "trigger", "player", "ragdoll" )
				.Run();

			Camera.Transform.LocalPosition = Vector3.Backward * (tr.Hit ? tr.Distance - 5.0f : MaxBoomLength);
		}

		Update( eyeHeight );
	}

	protected override void OnStart()
	{
		// Create a highlight component if it doesn't exist on the camera.
		Camera.Components.GetOrCreate<Highlight>();
		base.OnStart();
	}

	private void Update( float eyeHeight )
	{
		var baseFov = GameSettingsSystem.Current.FieldOfView;
		FieldOfViewOffset = 0;

		if ( !Player.IsValid() )
			return;

		if ( Player.CurrentEquipment.IsValid() )
		{
			if ( Player.CurrentEquipment?.Tags.Has( "aiming" ) ?? false )
			{
				FieldOfViewOffset += AimFovOffset;
			}
		}

		// deathcam, "zoom" at target.
		if ( Player.HealthComponent.State == LifeState.Dead )
		{
			FieldOfViewOffset += AimFovOffset;
		}

		Boom.Transform.LocalPosition = Vector3.Zero.WithZ( eyeHeight );

		TargetFieldOfView = TargetFieldOfView.LerpTo( baseFov + FieldOfViewOffset, Time.Delta * 5f );
		Camera.FieldOfView = TargetFieldOfView;

		if ( Input.Pressed( "View" ) )
		{
			Mode = Mode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
		}
	}

	void OnModeChanged()
	{
		SetBoomLength( Mode == CameraMode.FirstPerson ? 0.0f : ThirdPersonDistance );

		var firstPersonPOV = Mode == CameraMode.FirstPerson && Player.IsViewer;
		Player.Body?.SetFirstPersonView( firstPersonPOV );

		if ( firstPersonPOV )
			Player.CreateViewModel( false );
		else
			Player.ClearViewModel();
	}

	private void SetBoomLength( float length )
	{
		MaxBoomLength = length;
	}
}
