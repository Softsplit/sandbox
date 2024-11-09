public sealed partial class GameManager
{
	[ConCmd( "spawn" )]
	public static void Spawn( string modelname )
	{
		var player = Player.FindLocalPlayer();
		if ( !player.IsValid() )
			return;

		var tr = Game.ActiveScene.Trace.Ray( player.EyeTransform.Position, player.EyeTransform.Position + player.EyeTransform.Rotation.Forward * 500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( player.GameObject )
			.Run();

		var modelRotation = Rotation.From( new Angles( 0, player.EyeTransform.Rotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		SpawnModel( modelname, tr.EndPosition, modelRotation, player.GameObject );
		Sandbox.Services.Stats.Increment( "spawn.model", 1, modelname );
	}

	[Broadcast]
	static async void SpawnModel( string modelname, Vector3 endPos, Rotation modelRotation, GameObject playerObject )
	{
		if ( !Networking.IsHost || !Networking.IsActive )
			return;

		//
		// Does this look like a package?
		//
		if ( modelname.Count( x => x == '.' ) == 1 && !modelname.EndsWith( ".vmdl", StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", StringComparison.OrdinalIgnoreCase ) )
		{
			modelname = await SpawnPackageModel( modelname, playerObject );
			if ( modelname == null )
				return;
		}

		var model = Model.Load( modelname );
		if ( model == null || model.IsError )
			return;

		var go = new GameObject
		{
			WorldPosition = endPos + Vector3.Down * model.PhysicsBounds.Mins.z,
			WorldRotation = modelRotation,
			Tags = { "solid" }
		};

		var prop = go.AddComponent<Prop>();
		prop.Model = model;

		var propHelper = go.AddComponent<PropHelper>();

		if ( prop.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
		{
			renderer.CreateBoneObjects = true;
		}

		var rb = propHelper.Rigidbody;
		if ( rb.IsValid() )
		{
			// If there's no physics model, create a simple OBB
			foreach ( var shape in rb.PhysicsBody.Shapes )
			{
				if ( !shape.IsMeshShape )
					continue;

				var newCollider = go.AddComponent<BoxCollider>();
				newCollider.Scale = model.PhysicsBounds.Size;
			}
		}

		go.NetworkSpawn();
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );
	}

	static async Task<string> SpawnPackageModel( string packageName, GameObject source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.TypeName != "model" || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid() ) return null; // source gameobject died or disconnected or something

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}

	public static GameObject SpawnGetModel( string ModelPath, Vector3 position, Rotation rotation )
	{
		var go = new GameObject
		{
			WorldPosition = position,
			WorldRotation = rotation
		};

		var prop = go.AddComponent<Prop>();
		prop.Model = Model.Load( ModelPath );

		var propHelper = go.AddComponent<PropHelper>();
		propHelper.Invincible = true;

		if ( prop.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
		{
			renderer.CreateBoneObjects = true;
		}

		var rb = propHelper.Rigidbody;
		if ( rb.IsValid() )
		{
			foreach ( var shape in rb.PhysicsBody.Shapes )
			{
				if ( !shape.IsMeshShape )
					continue;

				var newCollider = go.AddComponent<BoxCollider>();
				newCollider.Scale = prop.Model.PhysicsBounds.Size;
			}
		}

		go.Tags.Add( "solid", "wheel" );
		go.NetworkSpawn();
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );
		go.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		go.Network.AssignOwnership( Connection.Host );

		return go;
	}
}
