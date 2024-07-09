using System.Threading.Tasks;

public partial class GameManager : Component, Component.INetworkListener
{
	/// <summary>
	/// Currently active game object.
	/// </summary>
	public static GameManager Current { get; protected set; }

	protected override void OnStart()
	{
		Current = this;
	}

	/// <summary>
	/// This GameObject is probably a pawn, and would like to be placed on a spawnpoint.
	/// If you were making a team based game you'd want to choose the spawn based on team.
	/// Or not even call this. Up to you. Added as a convenience.
	/// </summary>
	public virtual void MoveToSpawnpoint( GameObject pawn )
	{
		var spawnpoint = Scene.GetAllComponents<SpawnPoint>()
			.OrderBy( x => Guid.NewGuid() )
			.FirstOrDefault();

		if ( spawnpoint == null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {pawn}!" );
			return;
		}

		pawn.Transform.Position = spawnpoint.Transform.Position;
		pawn.Transform.Rotation = spawnpoint.Transform.Rotation;
	}

	/// <summary>
	/// A GameObject has been killed. This is usually a pawn but anything can call it.
	/// </summary>
	public void OnKilled( GameObject pawn )
	{
		if ( pawn.Network.OwnerConnection != null )
		{
			OnKilled( pawn.Network.OwnerConnection, pawn );
			return;
		}
	}

	/// <summary>
	/// An entity, which is a pawn, and has a client, has been killed.
	/// </summary>
	public void OnKilled( Connection client, GameObject pawn )
	{
		Log.Info( $"{client.Name} was killed" );

		/*
		if ( pawn.LastAttacker != null )
		{
			if ( pawn.LastAttacker.Client != null )
			{
				OnKilledMessage( pawn.LastAttacker.Client.SteamId, pawn.LastAttacker.Client.Name, client.SteamId, client.Name, pawn.LastAttackerWeapon?.ClassName );
			}
			else
			{
				OnKilledMessage( pawn.LastAttacker.NetworkIdent, pawn.LastAttacker.ToString(), client.SteamId, client.Name, "killed" );
			}
		}
		else
		{
			OnKilledMessage( 0, "", client.SteamId, client.Name, "died" );
		}
		*/
	}

	[Broadcast]
	public async void MountPackage( string packageName )
	{
		await MountPackageAsync( packageName );
	}

	private static async Task<string> MountPackageAsync( string packageName )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
		{
			// Handle error or log
			return null;
		}

		// Downloads if not downloaded, mounts if not mounted
		await package.MountAsync();

		return package.GetMeta( "PrimaryAsset", "" );
	}

	[ConCmd( "spawn" )]
	public static async void Spawn( string modelname )
	{
		var owner = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();

		if ( owner == null )
			return;

		var tr = Game.ActiveScene.Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( owner.GameObject )
			.Run();

		var modelRotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		Model model;

		if ( modelname.Contains( "." ) && !modelname.EndsWith( ".vmdl", System.StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", System.StringComparison.OrdinalIgnoreCase ) )
		{
			Current.MountPackage( modelname );
			model = Model.Load( await MountPackageAsync( modelname ) );
		}
		else
		{
			model = Model.Load( modelname );
		}

		if ( model == null || model.IsError )
			return;

		var ent = new GameObject
		{
			Name = modelname.Substring( modelname.LastIndexOf( '/' ) + 1, modelname.LastIndexOf( '.' ) - modelname.LastIndexOf( '/' ) - 1 )
		};

		ent.Tags.Add( "solid" );

		ent.Transform.Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z;
		ent.Transform.Rotation = modelRotation;

		var prop = ent.Components.Create<Prop>();
		prop.Model = model;
		ent.NetworkSpawn();
		// TODO: make it editable for everyone
		// ent.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		// ent.Network.DropOwnership(); 

		Sandbox.Services.Stats.Increment( "spawn.model", 1, modelname );
	}

	[ConCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		var owner = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();

		if ( owner == null )
			return;

		var entityType = TypeLibrary.GetType<Component>( entName )?.TargetType;
		if ( entityType == null )
			return;

		if ( !TypeLibrary.HasAttribute<SpawnableAttribute>( entityType ) )
			return;

		var tr = Game.ActiveScene.Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 200 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( owner.GameObject )
			.Size( 2 )
			.Run();

		var ent = new GameObject
		{
			Name = entName
		};

		ent.Tags.Add( "solid" );

		/*
	    if ( ent is BaseCarriable && owner.Inventory != null )
	    {
		   if ( owner.Inventory.Add( ent, true ) )
			   return;
	    }
		*/

		ent.Components.Create( TypeLibrary.GetType<Component>( entName ) );

		ent.Transform.Position = tr.EndPosition;
		ent.Transform.Rotation = Rotation.From( new Angles( 0, Game.ActiveScene.Camera.Transform.Rotation.Angles().yaw, 0 ) );

		ent.NetworkSpawn();
		ent.Network.DropOwnership();

		//Log.Info( $"ent: {ent}" );
	}
}
