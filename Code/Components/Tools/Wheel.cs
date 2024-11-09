[Library( "tool_wheel", Description = "A wheel that you can turn on and off (but actually can't yet)", Group = "construction" )]
public class Wheel : BaseTool
{
	GameObject previewObject;
	RealTimeSince timeSinceDisabled;

	protected override void OnUpdate()
	{
		if ( timeSinceDisabled < Time.Delta * 5f || !Parent.IsValid() )
			return;

		var trace = Parent.BasicTraceTool();

		if ( !trace.Hit || trace.Tags.Contains( "wheel" ) )
		{
			previewObject?.DestroyImmediate();
			previewObject = null;
			return;
		}

		if ( !previewObject.IsValid() )
		{
			previewObject = new GameObject();

			var renderer = previewObject.Components.Create<ModelRenderer>();
			renderer.Tint = Color.White.WithAlpha( 0.5f );
			renderer.Model = Model.Load( "models/citizen_props/wheel01.vmdl" );
		}

		PositionWheel( previewObject, trace );
	}

	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit || !trace.GameObject.IsValid() )
			return false;

		if ( Input.Pressed( "attack1" ) )
		{
			if ( trace.Tags.Contains( "wheel" ) )
				return true;

			var wheel = SpawnWheel( trace );

			PropHelper propHelper = wheel.Components.Get<PropHelper>();
			if ( !propHelper.IsValid() )
				return true;

			propHelper.Hinge( trace.GameObject, trace.EndPosition, trace.Normal );

			return true;
		}

		return false;
	}

	void PositionWheel( GameObject wheel, SceneTraceResult trace )
	{
		wheel.WorldPosition = trace.HitPosition + trace.Normal * 8f;
		wheel.WorldRotation = Rotation.LookAt( trace.Normal ) * Rotation.From( new Angles( 0, 90, 0 ) );
	}

	protected override void OnDestroy()
	{
		previewObject?.DestroyImmediate();
		base.OnDestroy();
	}

	public override void Disabled()
	{
		timeSinceDisabled = 0;
		previewObject?.DestroyImmediate();
	}

	GameObject SpawnWheel( SceneTraceResult trace )
	{
		var go = new GameObject();

		PositionWheel( go, trace );

		var prop = go.AddComponent<Prop>();
		prop.Model = Model.Load( "models/citizen_props/wheel01.vmdl" );

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

		return go;
	}
}
