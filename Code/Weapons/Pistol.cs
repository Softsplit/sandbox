[Library( "weapon_pistol", Title = "Pistol" )]
partial class Pistol : Weapon
{
	public RealTimeSince TimeSinceDischarge { get; set; }

	public override void ActiveStart()
	{
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		ViewModel?.Set( "b_deploy", true );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack();
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		Owner.ModelRenderer?.Set( "b_attack", true );
		ViewModel?.Set( "b_attack", true );

		ShootEffects();
		// Sound.Play( "sounds/balloon_pop_cute.sound", WorldPosition );
		ShootBullet( 0.05f, 1.5f, 9.0f, 3.0f );
	}

	private void Discharge()
	{
		if ( TimeSinceDischarge < 0.5f )
			return;

		TimeSinceDischarge = 0;

		var pos = Muzzle.WorldPosition;
		var rot = Muzzle.WorldRotation;

		ShootEffects();
		// Sound.Play( "sounds/balloon_pop_cute.sound", WorldPosition );

		ShootBullet( pos, rot.Forward, 0.05f, 1.5f, 9.0f, 3.0f );

		// TODO: Figure out what this is
		// ApplyAbsoluteImpulse( rot.Backward * 200.0f );
	}


	// TODO: Implement this
	/*
	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Speed > 500.0f )
		{
			Discharge();
		}
	}
	*/
}
