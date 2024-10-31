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

	[Broadcast]
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

		if ( ViewModel.Tags.Has( "viewer" ) )
			return;

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
}
