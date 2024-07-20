namespace Softsplit;

/// <summary>
/// Basic tool functionality
/// </summary>
public abstract class ToolComponent : InputWeaponComponent
{
	public VectorLineRenderer LineRenderer { get; set; }
	public bool AlwaysEnabledMenu { get; set; }
	public string ToolName { get; set; } = "";
	public string ToolDes { get; set; } = "";

	public Ray WeaponRay => Equipment.Owner.AimRay;

	private ToolGunHandler toolGunHandler;

	protected override void OnStart()
	{
		InputActions.Add( "Attack2" );
		InputActions.Add( "ToolGunMenu" );

		LineRenderer = Components.GetOrCreate<VectorLineRenderer>();
		LineRenderer.Points = new List<Vector3>{Vector3.Zero};
		LineRenderer.Color = Color.Cyan;
		LineRenderer.Width = 0.1f;
		LineRenderer.Noise = 0.2f;
		Start();
	}

	[Sync, Property] public float RayActive { get; set; }
	[Property] public float RayTime { get; set; } = 0.5f;

	protected override void OnUpdate()
	{
		Update();

		RayActive -= Time.Delta;

		LineRenderer.Enabled = RayActive > 0;

		LineRenderer.Points[0] = Effector.Muzzle.Transform.Position;
	}

	protected override void OnInputUpdate()
	{
		if ( !toolGunHandler.IsValid() )
			toolGunHandler = Components.Get<ToolGunHandler>();

		if ( Input.Pressed( "Attack1" ) )
			PrimaryAction();

		if ( Input.Pressed( "Attack2" ) )
			SecondaryAction();

		if ( AlwaysEnabledMenu )
			toolGunHandler.ActiveToolMenu.Enabled = Equipment.IsDeployed;
		else if ( Input.Pressed( "ToolGunMenu" ) )
			toolGunHandler.ActiveToolMenu.Enabled = !toolGunHandler.ActiveToolMenu.Enabled;
	}

	protected virtual void Start()
	{

	}

	protected virtual void Update()
	{
		
	}

	protected virtual void PrimaryAction()
	{

	}

	protected virtual void SecondaryAction()
	{

	}


	protected IEquipment Effector
	{
		get
		{
			if ( IsProxy || !Equipment.ViewModel.IsValid() )
				return Equipment;

			return Equipment.ViewModel;
		}
	}

	private GameObject p1;
	private GameObject p2;

	protected SceneTraceResult Trace()
	{
		return Scene.Trace.Ray( WeaponRay.Position, WeaponRay.Position + WeaponRay.Forward * 500 )
		.UseHitboxes()
		.IgnoreGameObjectHierarchy( GameObject.Root )
		.WithoutTags( "trigger", "invis", "ragdoll", "movement", "player_clip", "player" )
		.Run();
	}

	[Broadcast]
	public void Recoil( Vector3 effectPoint )
	{
		if ( p1.IsValid() )
		{
			p1.Destroy();
			p2.Destroy();
		}

		RayActive = 0.2f;

		LineRenderer.Points = GetSpacedPoints( Effector.Muzzle.Transform.Position, effectPoint, 10);
		Log.Info(LineRenderer.Points.Count);

		Sound.Play( "sounds/guns/gun_dryfire.sound", Transform.Position );

		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
			Equipment.Owner.BodyRenderer.Set( "b_attack", true );

		if ( Equipment.ViewModel.IsValid() )
			Equipment.ViewModel.ModelRenderer.Set( "b_attack", true );
	}
	public static List<Vector3> GetSpacedPoints(Vector3 start, Vector3 end, int numberOfPoints)
    {
        List<Vector3> points = new List<Vector3>();

        float step = 1.0f / (numberOfPoints - 1);

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i * step;
            Vector3 point = Vector3.Lerp(start, end, t);
            points.Add(point);
        }

        return points;
    }
}
