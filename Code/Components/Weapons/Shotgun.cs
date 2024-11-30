[Spawnable, Library( "weapon_shotgun" )]
partial class Shotgun : BaseWeapon
{
	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		BroadcastAttackPrimary();

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();

		//
		// Shoot the bullets
		//
		ShootBullets( 10, 0.1f, 10.0f, 9.0f, 3.0f );
	}

	[Rpc.Broadcast]
	private void BroadcastAttackPrimary()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
		Sound.Play( "rust_pumpshotgun.shoot", WorldPosition );
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		BroadcastAttackSecondary();

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();

		//
		// Shoot the bullets
		//
		ShootBullets( 20, 0.4f, 20.0f, 8.0f, 3.0f );
	}

	[Rpc.Broadcast]
	private void BroadcastAttackSecondary()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
		Sound.Play( "rust_pumpshotgun.shootdouble", WorldPosition );
	}

	// TODO: Probably should unify these particle methods + make it work for world models

	protected override void ShootEffects()
	{
		base.ShootEffects();

		Particles.CreateParticleSystem( "particles/pistol_ejectbrass.vpcf", Attachment( "ejection_point" ) );
	}

	protected virtual void DoubleShootEffects()
	{
		Particles.CreateParticleSystem( "particles/pistol_muzzleflash.vpcf", Attachment( "muzzle" ) );

		ViewModel?.Renderer?.Set( "fire_double", true );
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		FinishReload();
	}

	protected virtual void FinishReload()
	{
		ViewModel?.Renderer?.Set( "reload_finished", true );
	}
}
