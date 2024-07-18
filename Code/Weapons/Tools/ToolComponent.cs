using Sandbox.Events;

namespace Softsplit;

/// <summary>
/// Basic tool functionality
/// </summary>
public abstract class ToolComponent : InputWeaponComponent
{
	ToolGunHandler toolGunHandler;
	public Ray WeaponRay => Equipment.Owner.AimRay;

	public LineRenderer lineRenderer;

	
	protected override void OnStart()
	{
		InputActions.Add("Attack2");
		InputActions.Add("ToolGunMenu");
		lineRenderer = Components.GetOrCreate<LineRenderer>();
		lineRenderer.Color = Color.Cyan;
		lineRenderer.Width = 0.1f;

	}

	[Sync, Property] public float RayActive {get;set;}

	[Property] public float RayTime {get;set;} = 0.1f;


	protected override void OnUpdate()
	{

		RayActive -= Time.Delta;

		lineRenderer.Enabled = RayActive > 0;
	}
	protected override void OnInputUpdate()
	{
		
		if(toolGunHandler == null)
		{
			toolGunHandler = Components.Get<ToolGunHandler>();

		}

		if(Input.Pressed("Attack1"))
			PrimaryAction();
		if(Input.Pressed("Attack2")) 
			SecondaryAction();
		if(Input.Pressed("ToolGunMenu")) 
			toolGunHandler.ActiveToolMenu.Enabled = !toolGunHandler.ActiveToolMenu.Enabled;
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
	GameObject p1;
	GameObject p2;
	[Broadcast]
	public void Recoil(Vector3 effectPoint)
	{
		
		if(p1 != null)
		{
			p1.Destroy();
			p2.Destroy();
		}
		
		RayActive = RayTime;
		
		p1 = new GameObject();
		p1.Transform.Position = Effector.Muzzle.Transform.Position;
		p2 = new GameObject();
		p2.Transform.Position = effectPoint;
		lineRenderer.Points = new List<GameObject>{p1,p2};
		

		Sound.Play( "sounds/guns/gun_dryfire.sound", Transform.Position );
		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
			Equipment.Owner.BodyRenderer.Set( "b_attack", true );

		if ( Equipment.ViewModel.IsValid() )
			Equipment.ViewModel.ModelRenderer.Set( "b_attack", true );

		
	}


}