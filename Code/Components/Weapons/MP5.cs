[Spawnable, Library( "weapon_mp5", Title = "MP5" )]
partial class MP5 : BaseWeapon
{
	protected ParticleSystem EjectBrass => Cloud.ParticleSystem( "facepunch.9mm_ejectbrass" );

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		Owner?.Controller?.Renderer?.Set( "b_attack", true );
		ViewModel?.Renderer?.Set( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		Sound.Play( "rust_smg.shoot", WorldPosition );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.1f, 1.5f, 5.0f, 3.0f );
	}

	public override void OnControl()
	{
		base.OnControl();

		var attack_hold = !IsReloading && Input.Down( "attack1" ) ? 1.0f : 0.0f;
		Owner?.Controller?.Renderer?.Set( "attack_hold", attack_hold );
		ViewModel?.Renderer?.Set( "attack_hold", attack_hold );
	}

	// TODO: Probably should unify these particle methods + make it work for world models

	protected override void ShootEffects()
	{
		base.ShootEffects();

		if ( ViewModel.Tags.Has( "viewer" ) )
			return;

		var go = new GameObject
		{
			Name = EjectBrass.Name,
			Parent = ViewModel.GameObject,
			WorldTransform = ViewModel?.Renderer?.GetAttachment( "eject" ) ?? default
		};

		var legacyParticleSystem = go.AddComponent<LegacyParticleSystem>();
		legacyParticleSystem.Particles = EjectBrass;
		legacyParticleSystem.ControlPoints = new()
		{
			new ParticleControlPoint { GameObjectValue = go, Value = ParticleControlPoint.ControlPointValueInput.GameObject }
		};

		go.NetworkSpawn();
		go.DestroyAsync();
	}
}
