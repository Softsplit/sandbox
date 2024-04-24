using Sandbox;
using System.Linq;

[Spawnable]
[Library( "directional_gravity", Title = "Directional Gravity" )]
public partial class DirectionalGravity : Prop
{
	bool enabled = false;

	protected override void OnStart()
	{
		DeleteOthers();

		Model = Model.Load( "models/arrow.vmdl" );
		enabled = true;
	}

	private void DeleteOthers()
	{
		// Only allow one of these to be spawned at a time
		foreach ( var ent in Scene.GetAllComponents<DirectionalGravity>()
			.Where( x => x.IsValid() && x != this ) )
		{
			ent.Destroy();
		}
	}

	protected override void OnDestroy()
	{
		Scene.PhysicsWorld.Gravity = Vector3.Down * 800.0f;
		enabled = false;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( !enabled )
			return;

		if ( !this.IsValid() )
			return;

		Scene.PhysicsWorld.Gravity = Transform.Rotation.Down * 800.0f;
	}
}
