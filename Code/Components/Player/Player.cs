using static Sandbox.Component;

/// <summary>
/// Holds player information like health
/// </summary>
public sealed class Player : Component, IDamageable, BodyController.IEvents
{
	public static Player FindLocalPlayer()
	{
		return Game.ActiveScene.GetAllComponents<Player>().Where( x => !x.IsProxy ).FirstOrDefault();
	}

	[RequireComponent] public BodyController Controller { get; set; }

	[Property] public GameObject Body { get; set; }
	[Property, Range( 0, 100 ), Sync] public float Health { get; set; } = 100;

	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }
	[Property] public BodyController BodyController { get; set; }
	[Property] public PlayerInventory PlayerInventory { get; set; }

	public Ray AimRay => new( Scene.Camera.WorldPosition, Scene.Camera.Transform.World.Forward * 10000f );

	public bool IsDead => Health <= 0;

	public Transform EyeTransform => Controller.EyeTransform;

	protected override void OnStart()
	{
		ModelRenderer = Components.GetInChildrenOrSelf<SkinnedModelRenderer>();
		BodyController = Components.GetInChildrenOrSelf<BodyController>();
		PlayerInventory = Components.GetInChildrenOrSelf<PlayerInventory>();
	}

	/// <summary>
	/// Creates a ragdoll but it isn't enabled
	/// </summary>
	[Broadcast]
	void CreateRagdoll()
	{
		var ragdoll = Controller.CreateRagdoll();
		if ( !ragdoll.IsValid() ) return;

		var corpse = ragdoll.AddComponent<PlayerCorpse>();
		corpse.Connection = Network.Owner;
		corpse.Created = DateTime.Now;
	}

	[Broadcast( NetPermission.OwnerOnly )]
	void CreateRagdollAndGhost()
	{
		if ( !Networking.IsHost ) return;

		var go = new GameObject( false, "Observer" );
		go.Components.Create<PlayerObserver>();
		go.NetworkSpawn( Rpc.Caller );
	}

	public void TakeDamage( float amount )
	{
		if ( IsProxy ) return;
		if ( Health <= 0 ) return;

		Health -= amount;

		IPlayerEvent.Post( x => x.OnTakeDamage( this, amount ) );

		if ( Health <= 0 )
		{
			Health = 0;
			Death();
		}
	}

	void Death()
	{
		CreateRagdoll();
		CreateRagdollAndGhost();

		IPlayerEvent.Post( x => x.OnDied( this ) );

		GameObject.Destroy();
	}

	void IDamageable.OnDamage( in DamageInfo damage )
	{
		TakeDamage( damage.Damage );
	}

	void BodyController.IEvents.OnEyeAngles( ref Angles ang )
	{
		var player = Components.Get<Player>();
		var angles = ang;
		Scene.RunEvent<IPlayerEvent>( x => x.OnCameraMove( player, ref angles ) );
		ang = angles;
	}

	void BodyController.IEvents.PostCameraSetup( CameraComponent camera )
	{
		var player = Components.Get<Player>();
		IPlayerEvent.Post( x => x.OnCameraSetup( player, camera ) );
		IPlayerEvent.Post( x => x.OnCameraPostSetup( player, camera ) );
	}

	void BodyController.IEvents.OnLanded( float distance, Vector3 impactVelocity )
	{
		var player = Components.Get<Player>();
		IPlayerEvent.Post( x => x.OnLand( player, distance, impactVelocity ) );
	}
}
