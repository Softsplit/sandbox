[Spawnable, Library( "weapon_pistol" )]
partial class Pistol : BaseWeapon, Component.ICollisionListener
{
	public RealTimeSince TimeSinceDischarge { get; set; }

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( "attack1" );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		Owner?.Controller?.Renderer?.Set( "b_attack", true );
		// ViewModelEntity?.SetAnimParameter( "b_attack", true );

		ShootEffects();
		GameObject.PlaySound( new SoundEvent( "rust_pistol.shoot" ) );
		ShootBullet( 0.05f, 1.5f, 9.0f, 3.0f );
	}

	private void Discharge()
	{
		if ( TimeSinceDischarge < 0.5f )
			return;

		TimeSinceDischarge = 0;

		// var muzzle = GetAttachment( "muzzle" ) ?? default;
		// var pos = muzzle.Position;
		// var rot = muzzle.Rotation;

		ShootEffects();
		GameObject.PlaySound( new SoundEvent( "rust_pistol.shoot" ) );
		// ShootBullet( pos, rot.Forward, 0.05f, 1.5f, 9.0f, 3.0f );

		// ApplyAbsoluteImpulse( rot.Backward * 200.0f );
	}

	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( collision.Contact.NormalSpeed > 500.0f )
		{
			Discharge();
		}
	}
}
