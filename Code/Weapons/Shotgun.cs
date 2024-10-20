using Sandbox;

[Spawnable]
[Library( "weapon_shotgun", Title = "Shotgun" )]
partial class Shotgun : Weapon
{

	[Property] public ParticleSystem EjectBrass;
	[Property] public GameObject BrassVM;
	[Property] public GameObject BrassWM;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		ShootEffects();
		//PlaySound( "rust_pumpshotgun.shoot" );

		//
		// Shoot the bullets
		//
		ShootBullets( 10, 0.1f, 10.0f, 9.0f, 3.0f );
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		DoubleShootEffects();
		//PlaySound( "rust_pumpshotgun.shootdouble" );

		//
		// Shoot the bullets
		//
		ShootBullets( 20, 0.4f, 20.0f, 8.0f, 3.0f );
	}

	protected override void ShootEffects()
	{
		base.ShootEffects();
		CreateParticleSystem(EjectBrass.ResourcePath, UseWorldModel ? BrassWM.WorldPosition : BrassVM.WorldPosition, UseWorldModel ? BrassWM.WorldRotation : BrassVM.WorldRotation);
	}

	protected virtual void DoubleShootEffects()
	{
		ViewModel?.Set( "fire_double", true );
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		FinishReload();
	}

	[Broadcast]
	protected virtual void FinishReload()
	{
		ViewModel?.Set( "reload_finished", true );
	}
}
