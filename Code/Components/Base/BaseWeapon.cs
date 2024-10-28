/// <summary>
/// A common base we can use for weapons so we don't have to implement the logic over and over
/// again. Feel free to not use this and to implement it however you want to.
/// </summary>
[Icon( "sports_martial_arts" )]
public partial class BaseWeapon : Component
{
	[Property] public float PrimaryRate { get; set; } = 5.0f;
	[Property] public float SecondaryRate { get; set; } = 15.0f;
	[Property] public float ReloadTime { get; set; } = 3.0f;

	[Sync] public bool IsReloading { get; set; }
	[Sync] public RealTimeSince TimeSinceReload { get; set; }
	[Sync] public RealTimeSince TimeSinceDeployed { get; set; }
	[Sync] public RealTimeSince TimeSincePrimaryAttack { get; set; }
	[Sync] public RealTimeSince TimeSinceSecondaryAttack { get; set; }

	public Player Owner => Scene.GetAllComponents<Player>().Where( x => x.Network.OwnerId == Network.OwnerId ).FirstOrDefault();

	protected override void OnEnabled()
	{
		TimeSinceDeployed = 0;
	}

	protected override void OnFixedUpdate()
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			if ( IsProxy )
				return;

			if ( CanReload() )
			{
				Reload();
			}

			//
			// Reload could have changed our owner
			//
			if ( Owner == null )
				return;

			if ( CanPrimaryAttack() )
			{
				TimeSincePrimaryAttack = 0;
				AttackPrimary();
			}

			//
			// AttackPrimary could have changed our owner
			//
			if ( Owner == null )
				return;

			if ( CanSecondaryAttack() )
			{
				TimeSinceSecondaryAttack = 0;
				AttackSecondary();
			}
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
		// ViewModelEntity?.SetAnimParameter( "b_reload", true );

		// TODO - player third person model reload
	}

	[Broadcast]
	protected virtual void ShootEffects()
	{
		// Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		// ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public virtual bool CanReload()
	{
		if ( Owner == null || !Input.Down( "reload" ) ) return false;

		return true;
	}

	public virtual void Reload()
	{
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		Owner.Controller.Renderer.Set( "b_reload", true );

		StartReloadEffects();
	}

	public virtual bool CanPrimaryAttack()
	{
		if ( Owner == null || !Input.Down( "attack1" ) ) return false;

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual void AttackPrimary()
	{

	}

	public virtual bool CanSecondaryAttack()
	{
		if ( Owner == null || !Input.Down( "attack2" ) ) return false;

		var rate = SecondaryRate;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public virtual void AttackSecondary()
	{

	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocheting or something.
	/// </summary>
	public virtual IEnumerable<SceneTraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		// bool underWater = Trace.TestPoint( start, "water" );

		var trace = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.IgnoreGameObjectHierarchy( GameObject )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		/*
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );
		*/

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;

		//
		// Another trace, bullet going through thin material, penetrating water surface?
		//
	}

	public IEnumerable<SceneTraceResult> TraceMelee( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.IgnoreGameObjectHierarchy( GameObject );

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
			tr.Surface.DoBulletImpact( tr );

			if ( !tr.GameObject.IsValid() ) continue;

			if ( tr.Component is PropHelper prop )
			{
				prop.Damage( damage );
			}
			else if ( tr.Component is Player player )
			{
				player.TakeDamage( damage );
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
}
