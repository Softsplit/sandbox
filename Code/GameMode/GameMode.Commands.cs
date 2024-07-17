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

		Model model;

		if ( modelname.Contains( '.' ) && !modelname.EndsWith( ".vmdl", StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", StringComparison.OrdinalIgnoreCase ) )
		{
			MountPackage( modelname );

			if ( !Instance.cachedPackageList.Contains( modelname ) )
				Instance.cachedPackageList.Add( modelname );

			model = Model.Load( await MountPackageAsync( modelname ) );
		}
		else
		{
			model = Model.Load( modelname );
		}

		if ( model == null || model.IsError )
			return;

		var ent = new GameObject();
		ent.Transform.Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z;
		ent.Transform.Rotation = modelRotation;

		ent.Tags.Add( "solid" );

		var prop = ent.Components.Create<Prop>();
		prop.Model = model;

		foreach ( var shape in ent.Components.Get<Rigidbody>()?.PhysicsBody.Shapes )
		{
			if ( shape.IsMeshShape )
			{
				var collider = ent.Components.Create<BoxCollider>();
				collider.Center = model.PhysicsBounds.Center;
				collider.Scale = model.PhysicsBounds.Size;
			}
		}

		ent.NetworkSpawn();
		ent.Network.DropOwnership();

		owner.PlayerState.SpawnedPropsList.Add( ent );
		Stats.Increment( "spawn.model", 1, modelname );
	}
}
