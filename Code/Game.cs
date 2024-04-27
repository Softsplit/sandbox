public sealed class GameManager : Component
{
	/*
	[ConCmd( "spawn" )]
	public async Task Spawn( string modelname )
	{
		var owner = Scene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault().GameObject;

		if ( owner == null )
			return;

		var tr = Scene.Trace.Ray( Scene.Camera.Transform.Position, Scene.Camera.Transform.Position + Scene.Camera.Transform.Rotation.Forward * 500 )
			.UseHitboxes()
			.IgnoreGameObject( owner )
			.Run();

		var modelRotation = Rotation.From( new Angles( 0, Scene.Camera.Transform.Rotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		//
		// Does this look like a package?
		//
		if ( modelname.Count( x => x == '.' ) == 1 && !modelname.EndsWith( ".vmdl", System.StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", System.StringComparison.OrdinalIgnoreCase ) )
		{
			modelname = await SpawnPackageModel( modelname, owner );
			if ( modelname == null )
				return;
		}

		var model = Model.Load( modelname );
		if ( model == null || model.IsError )
			return;

		var ent = new GameObject
		{
			Name = modelname.Substring( modelname.LastIndexOf( '/' ) + 1, modelname.LastIndexOf( '.' ) - modelname.LastIndexOf( '/' ) - 1 )
		};

		ent.Transform.Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z;
		ent.Transform.Rotation = modelRotation;

		var prop = ent.Components.Create<Prop>();
		prop.Model = model;

		ent.NetworkSpawn();
		ent.Network.DropOwnership();

		Sandbox.Services.Stats.Increment( "spawn.model", 1, modelname );
	}

	async Task<string> SpawnPackageModel( string packageName, GameObject source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid ) return null; // source entity died or disconnected or something

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}

	[ConCmd( "spawn_entity" )]
	public void SpawnEntity( string entName )
	{
		var owner = Scene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault().GameObject;

		if ( owner == null )
			return;

		var entityType = TypeLibrary.GetType<Component>( entName )?.TargetType;
		if ( entityType == null )
			return;

		if ( !TypeLibrary.HasAttribute<SpawnableAttribute>( entityType ) )
			return;

		var tr = Scene.Trace.Ray( Scene.Camera.Transform.Position, Scene.Camera.Transform.Position + Scene.Camera.Transform.Rotation.Forward * 200 )
			.UseHitboxes()
			.IgnoreGameObject( owner )
			.Size( 2 )
			.Run();

		var ent = new GameObject
		{
			Name = entName
		};

	    if ( ent is BaseCarriable && owner.Inventory != null )
	    {
		   if ( owner.Inventory.Add( ent, true ) )
			   return;
	    }

		ent.Components.Create( TypeLibrary.GetType<Component>( entName ) );

		ent.Transform.Position = tr.EndPosition;
		ent.Transform.Rotation = Rotation.From( new Angles( 0, Scene.Camera.Transform.Rotation.Angles().yaw, 0 ) );

		ent.NetworkSpawn();
		ent.Network.DropOwnership();

		//Log.Info( $"ent: {ent}" );
	}
	*/
}
