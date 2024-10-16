using Sandbox;
using Softsplit;
using System.Collections.Generic;

public partial class Weapon : BaseWeapon
{
	[Property] public float ReloadTime = 3.0f;

	[Sync]
	public TimeSince TimeSinceReload { get; set; }

	[Sync]
	public bool IsReloading { get; set; }

	[Sync]
	public TimeSince TimeSinceDeployed { get; set; }

    [Property] public float PrimaryRate = 15.0f;
	[Property] public float SecondaryRate = 1.0f;
	[Property] public bool Hold = false;

    public float TimeSincePrimaryAttack = 100000f;
    public float TimeSinceSecondaryAttack = 100000f;

    public virtual bool CanPrimaryAttack()
    {
        return TimeSincePrimaryAttack > PrimaryRate;
    }

    public virtual bool CanSecondaryAttack()
    {
        return TimeSinceSecondaryAttack > PrimaryRate;
    }

    public virtual bool CanReload()
    {
        return !IsReloading;
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
		Log.Info(beenEnabled);
		if(beenEnabled)
			return;
		ActiveStart();
		beenEnabled = true;
		TimeSinceDeployed = 0;
	}

	public virtual void ActiveStart()
	{
		Log.Info("FUCK YOU");
	}

	bool PressedDown(string key)
	{
		return Hold ? Input.Down(key) : Input.Pressed(key);
	}

	public override void OnControl()
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

        TimeSincePrimaryAttack+=Time.Delta*100;
        TimeSinceSecondaryAttack+=Time.Delta*100;

        Log.Info(ReloadTime);

        /*
		if ( !IsReloading )
		{
			base.Simulate( owner );
		}

        Figure out the purpose of this
        */

        if(CanPrimaryAttack() && PressedDown("attack1"))
            AttackPrimary();
        else if(CanSecondaryAttack() && PressedDown("attack2"))
            AttackSecondary();
        else if(CanReload() && Input.Pressed("reload"))
            Reload();

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

		Owner.ModelRenderer?.Set("b_reload",true);
	}

	[Broadcast]
	protected virtual void ShootEffects()
	{
		CreateParticleSystem( "particles/pistol_muzzleflash.vpcf", Muzzle.WorldPosition, Muzzle.WorldRotation );

		ViewModel?.Set( "fire", true );

        WorldModel?.Set("fire", true);
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
		//
        /*
        if ( !underWater )
			trace = trace.WithAnyTags( "water" );

        TR- Re-impliment?
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
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			CreateImpactEffects( tr );

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//


            //Figure out the sandbox events shit
            /*
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
            */
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
		gameObject.WorldPosition = pos;
		gameObject.WorldRotation = rot;

		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );
		gameObject.Transform.ClearInterpolation();

		gameObject.DestroyAsync( decay );

		return p;
	}

    private DecalRenderer CreateDecal( Material material, Vector3 pos, Vector3 normal, float rotation, float size, float depth, float destroyTime = 3f, GameObject parent = null )
	{
		var gameObject = Scene.CreateObject();
		gameObject.WorldPosition = pos;
		gameObject.WorldRotation = Rotation.LookAt( -normal );
		if ( parent != null ) gameObject.SetParent( parent );

		gameObject.WorldRotation *= Rotation.FromAxis( Vector3.Forward, rotation );

		var decalRenderer = gameObject.Components.Create<DecalRenderer>();
		decalRenderer.Material = material;
		decalRenderer.Size = new( size, size, depth );

		gameObject.DestroyAsync( destroyTime );

		return decalRenderer;
	}

    private void CreateImpactEffects( SceneTraceResult tr )
	{
		var decalPath = Game.Random.FromList( tr.Surface.ImpactEffects.BulletDecal, "decals/bullethole.decal" );
		if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decalResource ) )
		{
			var decal = Game.Random.FromList( decalResource.Decals );

			CreateDecal( decal.Material, tr.EndPosition, tr.Normal, decal.Rotation.GetValue(), decal.Width.GetValue() / 1.5f, decal.Depth.GetValue(), 30f, tr.GameObject.Parent );
		}

		if ( !string.IsNullOrEmpty( tr.Surface.Sounds.Bullet ) )
		{
			Sound.Play( tr.Surface.Sounds.Bullet, tr.EndPosition );
		}
	}
}
