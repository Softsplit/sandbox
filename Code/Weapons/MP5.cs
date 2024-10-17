[Library( "weapon_mp5", Title = "MP5" )]
partial class MP5 : Weapon
{
	[Property] public ParticleSystem EjectBrass;
	[Property] public GameObject BrassVM;
	[Property] public GameObject BrassWM;

	public override void ActiveStart()
	{
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		Owner.ModelRenderer?.Set( "b_attack", true );
		ViewModel?.Set( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		// Sound.Play( "sounds/balloon_pop_cute.sound", WorldPosition );
		ShootBullet( 0.1f, 1.5f, 5.0f, 3.0f );
	}

	public override void OnControl()
	{
		base.OnControl();

		var attackHold = !IsReloading && Input.Down( "attack1" ) ? 1.0f : 0.0f;

		Owner.ModelRenderer?.Set( "attack_hold", attackHold );
		ViewModel?.Set( "attack_hold", attackHold );
	}

	[Broadcast]
	protected override void ShootEffects()
	{
		base.ShootEffects();
		CreateParticleSystem( EjectBrass.ResourcePath, UseWorldModel ? BrassWM.WorldPosition : BrassVM.WorldPosition, Rotation.Identity );
	}
}
