public partial class Weapon : BaseWeapon
{
	[Property] public float PrimaryRate { get; set; } = 15.0f;
	[Property] public float SecondaryRate { get; set; } = 1.0f;
	[Property] public float ReloadTime { get; set; } = 3.0f;
	[Property] public bool Hold { get; set; } = false;

	[Sync] public RealTimeSince TimeSinceReload { get; set; }
	[Sync] public RealTimeSince TimeSinceDeployed { get; set; }
	[Sync] public RealTimeSince TimeSincePrimaryAttack { get; set; }
	[Sync] public RealTimeSince TimeSinceSecondaryAttack { get; set; }
	[Sync] public bool IsReloading { get; set; }

	public virtual bool CanPrimaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack1" ) ) return false;

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual bool CanSecondaryAttack()
	{
		if ( !Owner.IsValid() || !Input.Down( "attack2" ) ) return false;

		var rate = SecondaryRate;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public virtual bool CanReload()
	{
		if ( !Owner.IsValid() || !Input.Down( "reload" ) ) return false;

		return true;
	}

	public virtual void Reload()
	{
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		Owner.ModelRenderer.Set( "b_reload", true );

		StartReloadEffects();
	}

	public virtual void AttackPrimary()
	{
	}

	public virtual void AttackSecondary()
	{
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "weapon" );
	}

	bool beenEnabled = false;

	public override void DoEnabled()
	{
		if ( beenEnabled )
			return;

		ActiveStart();

		beenEnabled = true;
		TimeSinceDeployed = 0;
	}

	public virtual void ActiveStart()
	{
	}

	public override void OnControl()
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( CanReload() )
			Reload();

		// Explanation: This was logic you had to do in order to
		// run custom weapon code on the entity system
		/*
		if ( !IsReloading )
		{
			base.Simulate( owner );
		}
        */

		if ( CanPrimaryAttack() )
		{
			TimeSincePrimaryAttack = 0;
			AttackPrimary();
		}

		if ( CanSecondaryAttack() )
		{
			TimeSinceSecondaryAttack = 0;
			AttackSecondary();
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
	}

	[Broadcast]
	public virtual void StartReloadEffects()
	{
		ViewModel?.Set( "b_reload", true );
		Owner.ModelRenderer?.Set( "b_reload", true );
	}

	[Broadcast]
	protected virtual void ShootEffects()
	{
		CreateParticleSystem( "particles/pistol_muzzleflash.vpcf", Muzzle.WorldPosition, Muzzle.WorldRotation );

		Owner.ModelRenderer?.Set( "b_attack", true );
		ViewModel?.Set( "b_attack", true );
		WorldModel?.Set( "b_attack", true );
	}

	public IEnumerable<SceneTraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.IgnoreGameObjectHierarchy( Owner.GameObject )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		// TODO: Reimplement when water component is added
		//
		/*
        if ( !underWater )
			trace = trace.WithAnyTags( "water" );
        */

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;
	}

	public IEnumerable<SceneTraceResult> TraceMelee( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.IgnoreGameObjectHierarchy( Owner.GameObject );

		var tr = trace.Run();

		if ( tr.Hit )
		{
			yield return tr;
		}
		else
		{
			trace = trace.Size( radius );

			tr = trace.Run();

			if ( tr.Hit )
			{
				yield return tr;
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 2500f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			CreateImpactEffects( tr );

			var damageInfo = new DamageInfo( damage, Owner.GameObject, GameObject );

			if ( tr.GameObject.Components.TryGet<Rigidbody>( out var rb ) )
			{
				rb.ApplyImpulseAt( tr.EndPosition, forward * 100 * force );
			}

			if ( tr.GameObject.Components.TryGet<ModelPhysics>( out var modelPhysics ) )
			{
				modelPhysics.PhysicsGroup.ApplyImpulse( forward * 100 * force );
			}

			if ( tr.GameObject.Components.TryGet<IDamageable>( out var damageable ) )
			{
				damageable.OnDamage( in damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		var ray = Owner.AimRay;
		ShootBullet( ray.Position, ray.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var ray = Owner.AimRay;

		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( ray.Position, ray.Forward, spread, force / numBullets, damage, bulletSize );
		}
	}

	public LegacyParticleSystem CreateParticleSystem( string particle, Vector3 pos, Rotation rot, float decay = 5f )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Name = particle;
		gameObject.WorldPosition = pos;
		gameObject.WorldRotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.ControlPoints = new()
		{
			new ParticleControlPoint { GameObjectValue = gameObject, Value = ParticleControlPoint.ControlPointValueInput.GameObject }
		};

		p.Particles = ParticleSystem.Load( particle );

		gameObject.Transform.ClearInterpolation();
		gameObject.DestroyAsync( decay );

		return p;
	}

	private DecalRenderer CreateDecal( Material material, Vector3 pos, Vector3 normal, float rotation, float size, float depth, float destroyTime = 3f, GameObject parent = null )
	{
		var gameObject = Scene.CreateObject();
		gameObject.Name = material.Name;
		gameObject.WorldPosition = pos;
		gameObject.WorldRotation = Rotation.LookAt( -normal );

		if ( parent != null )
			gameObject.SetParent( parent );

		gameObject.WorldRotation *= Rotation.FromAxis( Vector3.Forward, rotation );

		var decalRenderer = gameObject.Components.Create<DecalRenderer>();
		decalRenderer.Material = material;
		decalRenderer.Size = new( size, size, depth );

		gameObject.DestroyAsync( destroyTime );

		return decalRenderer;
	}

	public void CreateImpactEffects( SceneTraceResult tr )
	{
		var decalPath = Game.Random.FromList( tr.Surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );

		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalResource ) )
		{
			var decal = Game.Random.FromList( decalResource.Decals );

			if ( decal != null )
			{
				CreateDecal( decal.Material, tr.EndPosition, tr.Normal, decal.Rotation.GetValue(), decal.Width.GetValue() / 1.5f, decal.Depth.GetValue(), 30f, tr.GameObject );
			}
		}

		var particlePath = Game.Random.FromList( tr.Surface.ImpactEffects.Bullet, "particles/impact.generic.vpcf" );

		CreateParticleSystem( particlePath, tr.EndPosition, Rotation.LookAt( tr.Normal ) );

		if ( !string.IsNullOrEmpty( tr.Surface.Sounds.Bullet ) && tr.Surface.IsValid() )
		{
			Sound.Play( tr.Surface.Sounds.Bullet, tr.EndPosition );
		}
	}
}
