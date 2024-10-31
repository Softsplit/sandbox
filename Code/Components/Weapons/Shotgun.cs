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

	[Broadcast]
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

	[Broadcast]
	private void BroadcastAttackSecondary()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
		Sound.Play( "rust_pumpshotgun.shootdouble", WorldPosition );
	}

	// TODO: Probably should unify these particle methods + make it work for world models

	protected override void ShootEffects()
	{
		base.ShootEffects();

		var particleSystem = ParticleSystem.Load( "particles/pistol_ejectbrass.vpcf" );

		var go = new GameObject
		{
			Name = particleSystem.Name,
			Parent = ViewModel.GameObject,
			WorldTransform = ViewModel?.Renderer?.GetAttachment( "ejection_point" ) ?? default
		};

		var legacyParticleSystem = go.AddComponent<LegacyParticleSystem>();
		legacyParticleSystem.Particles = particleSystem;
		legacyParticleSystem.ControlPoints = new()
		{
			new ParticleControlPoint { GameObjectValue = go, Value = ParticleControlPoint.ControlPointValueInput.GameObject }
		};

		go.DestroyAsync();
	}

	protected virtual void DoubleShootEffects()
	{
		if ( ViewModel.Tags.Has( "viewer" ) )
			return;

		var particleSystem = ParticleSystem.Load( "particles/pistol_muzzleflash.vpcf" );

		var go = new GameObject
		{
			Name = particleSystem.Name,
			Parent = ViewModel.GameObject,
			WorldTransform = ViewModel?.Renderer?.GetAttachment( "muzzle" ) ?? default
		};

		var legacyParticleSystem = go.AddComponent<LegacyParticleSystem>();
		legacyParticleSystem.Particles = particleSystem;
		legacyParticleSystem.ControlPoints = new()
		{
			new ParticleControlPoint { GameObjectValue = go, Value = ParticleControlPoint.ControlPointValueInput.GameObject }
		};

		go.DestroyAsync();

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
