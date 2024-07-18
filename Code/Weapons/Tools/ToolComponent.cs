namespace Softsplit;

/// <summary>
/// Basic tool functionality
/// </summary>
public abstract class ToolComponent : InputWeaponComponent
{
	ToolGunHandler toolGunHandler;

	protected override void OnStart()
	{
		InputActions.Add( "Attack2" );
		InputActions.Add( "ToolGunMenu" );
	}

	protected override void OnInputUpdate()
	{
		toolGunHandler ??= Components.Get<ToolGunHandler>();

		if ( Input.Pressed( "Attack1" ) )
			PrimaryAction();

		if ( Input.Pressed( "Attack2" ) )
			SecondaryAction();

		if ( Input.Pressed( "ToolGunMenu" ) )
			toolGunHandler.ActiveToolMenu.Enabled = !toolGunHandler.ActiveToolMenu.Enabled;
	}

	protected virtual void PrimaryAction()
	{

	}

	protected virtual void SecondaryAction()
	{

	}
}
