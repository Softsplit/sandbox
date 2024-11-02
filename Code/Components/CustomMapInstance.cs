public sealed class CustomMapInstance : MapInstance
{
	protected override void OnCreateObject( GameObject go, MapLoader.ObjectEntry kv )
	{
		base.OnCreateObject( go, kv );

		if ( kv.TypeName.StartsWith( "prop" ) ) go.AddComponent<PropHelper>();
		if ( kv.TypeName == "ent_door" )
		{
			if ( !Networking.IsHost )
				return;

			Model resource = kv.GetResource<Model>( "model" );

			var skMdl = go.Components.Create<SkinnedModelRenderer>();
			skMdl.Model = resource;
			var mdlCollider = go.Components.Create<ModelCollider>();
			mdlCollider.Model = resource;

			var door = go.Components.Create<Door>();

			Angles movedir = kv.GetValue<Angles>( "movedir" );
			float distance = kv.GetValue<float>( "distance" );
			Vector3 origin = kv.GetValue<Vector3>( "pivot" );

			bool startslocked = kv.GetValue<bool>( "startslocked" );
			bool locked = kv.GetValue<bool>( "locked" );

			Door.DoorMoveType movedir_type = kv.GetValue<Door.DoorMoveType>( "movedir_type" );
			door.Axis = movedir;
			door.Distance = distance;
			door.MoveDirType = movedir_type;
			door.Locked = locked || startslocked;
			door.PivotPosition = go.Transform.World.PointToWorld( origin );
			door.Collider = mdlCollider;

			go.SetParent( Scene );
			go.NetworkSpawn( null );
		}
	}
}
