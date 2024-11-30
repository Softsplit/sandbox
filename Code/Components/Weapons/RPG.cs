[Spawnable, Library( "weapon_rpg" )]
partial class RPG : BaseWeapon
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
		ShootBullet( 0.1f, 1.5f, 5.0f, 3.0f );
	}

	[Rpc.Broadcast]
	private void BroadcastAttackPrimary()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
		Sound.Play( "rust_smg.shoot", WorldPosition );
	}

	public override void AttackSecondary()
	{
		// Grenade lob
	}

	protected override void ShootEffects()
	{
		base.ShootEffects();

		Particles.CreateParticleSystem( "particles/pistol_ejectbrass.vpcf", Attachment( "ejection_point" ) );
	}
}
