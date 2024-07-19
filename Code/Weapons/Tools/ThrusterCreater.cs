using Sandbox.Physics;

namespace Softsplit;
public class Thruster : ToolComponent
{
	protected override void Start()
	{
		ToolName = "Thruster";
		ToolDes = "Create Thrusters";
	}

	protected override void PrimaryAction()
	{
		var aim = DoTrace();
		if ( aim.GameObject == null )
			return;
		if ( aim.Body == null )
			return;
		if ( aim.Body.BodyType == PhysicsBodyType.Static || aim.GameObject.Components.GetInChildrenOrSelf<ThrusterEntity>() != null )
			return;
		GameObject thruster = new GameObject();
		thruster.Transform.Position = aim.HitPosition;
		thruster.Transform.Rotation = Rotation.LookAt( aim.Normal ) * Rotation.From( new Angles( 90, 0, 0 ) );

		ModelRenderer modelRenderer = thruster.Components.Create<ModelRenderer>();
		modelRenderer.Model = Model.Load( "models/thruster/thrusterprojector.vmdl" );

		ModelCollider modelCollider = thruster.Components.Create<ModelCollider>();
		modelCollider.Model = Model.Load( "models/thruster/thrusterprojector.vmdl" );

		thruster.Components.Create<ThrusterEntity>();

		thruster.Components.Create<Rigidbody>();

		// Is dosnt work, idk how fix
		PhysicsBody body = Scene.Trace.Ray( modelRenderer.Bounds.Center, modelRenderer.Bounds.Center ).Run().Body;

		Weld.CreateWeld( body, body.Transform.PointToLocal(aim.HitPosition), aim.Body, body.Transform.PointToLocal( aim.HitPosition ) );
	}
}
