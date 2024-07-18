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

	[Broadcast]
	public void Recoil()
	{
		Sound.Play( "sounds/guns/gun_dryfire.sound", Transform.Position );
		if ( Equipment.Owner.IsValid() && Equipment.Owner.BodyRenderer.IsValid() )
			Equipment.Owner.BodyRenderer.Set( "b_attack", true );

		if ( Equipment.ViewModel.IsValid() )
			Equipment.ViewModel.ModelRenderer.Set( "b_attack", true );
	}
}
