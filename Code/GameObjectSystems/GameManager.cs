using Sandbox.ModelEditor.Nodes;

public sealed partial class GameManager : GameObjectSystem<GameManager>, IPlayerEvent, Component.INetworkListener, ISceneStartup
{
	public GameManager( Scene scene ) : base( scene )
	{
	}

	void ISceneStartup.OnHostPreInitialize( SceneFile scene )
	{
		Log.Info( $"Sandbox Classic: Loading scene {scene.ResourceName}" );
	}

	void Component.INetworkListener.OnActive( Connection channel )
	{
		SpawnPlayerForConnection( channel );
	}

	public void SpawnPlayerForConnection( Connection channel )
	{
		// Find a spawn location for this player
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var playerGo = GameObject.Clone( "/prefabs/player.prefab", new CloneConfig { Name = $"Player - {channel.DisplayName}", StartEnabled = true, Transform = startLocation } );
		var player = playerGo.Components.Get<Player>( true );
		playerGo.NetworkSpawn( channel );

		IPlayerEvent.PostToGameObject( player.GameObject, x => x.OnSpawned() );
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{
		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}

		//
		// Failing that, spawn where we are
		//
		return Transform.Zero;
	}
	
	[Broadcast]
	public static void Explosion( Component owner, string particle, Vector3 position, float radius, float damage, float forceScale )
	{
		// Effects
		Sound.Play( "rust_pumpshotgun.shootdouble", position );
		Particles.Create( particle, position );

		// Damage, etc
		var overlaps = Game.ActiveScene.FindInPhysics(new Sphere( position, radius ) );

		foreach ( var obj in overlaps )
		{

			
			var distance = obj.WorldPosition.Distance( position );
			var distanceMul = 1.0f - Math.Clamp( distance / radius, 0.0f, 1.0f );
			
			var dmg = damage * distanceMul;

			// If the object isn't in line of sight, fuck it off
			var tr = Game.ActiveScene.Trace.Ray( position, obj.WorldPosition ).Run();
			if ( tr.Hit && tr.GameObject.IsValid() )
			{
				if ( !obj.Root.IsDescendant( tr.GameObject ) )
					continue;
			}

			foreach ( var damageable in obj.Components.GetAll<Component.IDamageable>(  ) )
			{
				damageable.OnDamage( new DamageInfo(dmg, null, null) );
			}


			
			// obj.DebugOverlay.Text( obj.WorldPosition, $"{dmg:0.00}", duration: 5f, overlay: true );


		}
	}
}
