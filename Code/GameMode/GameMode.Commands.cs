using Sandbox.Services;
using System.Threading.Tasks;

namespace Softsplit;

partial class GameMode : Component.INetworkListener
{
	// TODO: This code is hot garbage, rewrite all of this crap later

	[HostSync] private NetList<string> cachedPackageList { get; set; } = new();

	void INetworkListener.OnActive( Connection conn )
	{
		foreach ( var packageName in cachedPackageList )
		{
			MountPackage( packageName );
		}
	}

	[Broadcast]
	public static async void MountPackage( string packageName )
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

	private static EquipmentResource HandleViewModelPackage( string packageName, PlayerPawn owner )
	{
		string fileformat = packageName.Substring( packageName.IndexOf( '.' ) );
		if ( !fileformat.StartsWith( ".v_" ) ) return null;
		string resourceFileName = fileformat.Substring( fileformat.IndexOf( '_' ) + 1 );

		List<string> resourceNames = EquipmentResource.All.Select( item => item.ResourceName ).ToList();
		EquipmentResource vm_weapon = EquipmentResource.All
		.First( x => x.ResourceName == GameUtils.getClosestString( resourceNames, resourceFileName ) );
		owner.Inventory.Drop( null, vm_weapon );

		return vm_weapon ?? null;
	}

	[ConCmd( "spawn" )]
	public static async void Spawn( string modelname )
	{
		var owner = PlayerState.Local.PlayerPawn;


		if ( owner == null )
			return;

		var tr = Game.ActiveScene.Trace.Ray( owner.AimRay, 500f )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( owner.GameObject )
			.Run();

		var modelRotation = Rotation.From( new Angles( 0, owner.EyeAngles.yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		if ( HandleViewModelPackage( modelname, owner ) != null )
			return;

		Model model;

		var needsMounting = modelname.Contains( '.' )
		&& !modelname.EndsWith( ".vmdl", StringComparison.OrdinalIgnoreCase )
		&& !modelname.EndsWith( ".vmdl_c", StringComparison.OrdinalIgnoreCase );

		if ( needsMounting && !Instance.cachedPackageList.Contains( modelname ) )
		{
			MountPackage( modelname );
			Instance.cachedPackageList.Add( modelname );
		}
		model = Model.Load( needsMounting ? await MountPackageAsync( modelname ) : modelname );

		if ( model == null || model.IsError )
			return;

		var ent = new GameObject();
		ent.Transform.Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z;
		ent.Transform.Rotation = modelRotation;

		ent.Tags.Add( "solid" );

		var prop = ent.Components.Create<Prop>();
		prop.Model = model;

		foreach ( var shape in ent.Components.GetOrCreate<Rigidbody>()?.PhysicsBody.Shapes )
		{
			if ( shape.IsMeshShape )
			{
				var collider = ent.Components.Create<BoxCollider>();
				collider.Center = model.PhysicsBounds.Center;
				collider.Scale = model.PhysicsBounds.Size;
			}
		}

		ent.Tags.Add( "propcollide" );
		ent.Network.SetOwnerTransfer( OwnerTransfer.Takeover );
		ent.Components.Create<HighlightOutline>(false);
		ent.NetworkSpawn( null );
		ent.Network.DropOwnership();

		owner.PlayerState.AddPropToList( ent );
		Stats.Increment( "spawn.model", 1, modelname );
	}


	[ConCmd( "spawnent" )]
	public static async void SpawnEnt( string path )
	{
		var owner = PlayerState.Local.PlayerPawn;

		if ( owner == null )
			return;

		if ( PrefabLibrary.TryGetByPath( path, out var prefabFile ) )
		{
			var obj = SceneUtility.GetPrefabScene( prefabFile.Prefab ).Clone();

			var tr = Game.ActiveScene.Trace.Ray( owner.AimRay, 500f )
				.UseHitboxes()
				.IgnoreGameObjectHierarchy( owner.GameObject )
				.Run();

			var modelRotation = Rotation.From( new Angles( 0, owner.EyeAngles.yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );


			obj.Transform.Position = tr.EndPosition + Vector3.Down * -5;//obj.Mins.z
			obj.Transform.Rotation = modelRotation;

			obj.NetworkMode = NetworkMode.Object;
			obj.NetworkSpawn();

			owner.PlayerState.AddPropToList( obj );
			Stats.Increment( "spawn.model", 1, path );

		}
	}

}
