namespace Softsplit;

/// <summary>
/// Basic tool functionality
/// </summary>
public abstract class ToolComponent : InputWeaponComponent
{
	public LineRenderer LineRenderer { get; set; }
	public bool AlwaysEnabledMenu { get; set; }
	public string ToolName { get; set; } = "";
	public string ToolDes { get; set; } = "";

	public Ray WeaponRay => Equipment.Owner.AimRay;

	private ToolGunHandler toolGunHandler;

	protected override void OnStart()
	{
		InputActions.Add( "Attack2" );
		InputActions.Add( "ToolGunMenu" );

		LineRenderer = Components.GetOrCreate<LineRenderer>();
		LineRenderer.Color = Color.Cyan;
		LineRenderer.Width = 0.1f;

		Start();
	}

	[Sync, Property] public float RayActive { get; set; }
	[Property] public float RayTime { get; set; } = 0.1f;

	protected override void OnUpdate()
	{
		Update();

		RayActive -= Time.Delta;

		LineRenderer.Enabled = RayActive > 0;
	}

	protected override void OnInputUpdate()
	{
		if ( !toolGunHandler.IsValid() )
		{
			toolGunHandler = Components.Get<ToolGunHandler>();
			if ( ToolName != "" )
			{
				toolGunHandler.ToolGunUI.Enabled = true;
				toolGunHandler.ToolGunUI.ToolName = ToolName;
				toolGunHandler.ToolGunUI.ToolDes = ToolDes;
			}
			else
			{
				toolGunHandler.ToolGunUI.Enabled = false;
			}
		}

		if ( Input.Pressed( "Attack1" ) )
			PrimaryAction();

		if ( Input.Pressed( "Attack2" ) )
			SecondaryAction();

		if ( toolGunHandler.ActiveToolMenu != null )
		{
			if ( AlwaysEnabledMenu )
				toolGunHandler.ActiveToolMenu.Enabled = Equipment.IsDeployed;
			else if ( Input.Pressed( "ToolGunMenu" ) )
				toolGunHandler.ActiveToolMenu.Enabled = !toolGunHandler.ActiveToolMenu.Enabled;
		}
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

	[Broadcast]
	public void Recoil( Vector3 effectPoint )
	{
		if ( p1.IsValid() )
		{
			p1.Destroy();
			p2.Destroy();
		}

		RayActive = RayTime;

		p1 = new GameObject();
		p1.Transform.Position = Effector.Muzzle.Transform.Position;

		p2 = new GameObject();
		p2.Transform.Position = effectPoint;

		LineRenderer.Points = new List<GameObject> { p1, p2 };

		Sound.Play( "sounds/guns/gun_dryfire.sound", Transform.Position );

		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
			Equipment.Owner.BodyRenderer.Set( "b_attack", true );

		if ( Equipment.ViewModel.IsValid() )
			Equipment.ViewModel.ModelRenderer.Set( "b_attack", true );
	}
}
