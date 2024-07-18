using Sandbox.Events;

namespace Softsplit;

/// <summary>
/// Basic tool functionality
/// </summary>
public abstract class ToolComponent : InputWeaponComponent
{
	ToolGunHandler toolGunHandler;
	public Ray WeaponRay => Equipment.Owner.AimRay;
	protected override void OnStart()
	{
		InputActions.Add("Attack2");
		InputActions.Add("ToolGunMenu");
	}
	protected override void OnInputUpdate()
	{
		if(toolGunHandler == null) toolGunHandler = Components.Get<ToolGunHandler>();
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


}