namespace Tools;
public class ThrusterCreater : Tool
{
	protected override void OnUpdate()
	{
		if (!IsUsing()) 
			return;
		if ( !Input.Pressed( "attack1" ) )
			return;
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

		FixedJoint fixedJoint1 = thruster.Components.Create<FixedJoint>();
		fixedJoint1.Body = aim.Body.GetGameObject();

		FixedJoint fixedJoint2 = aim.Body.GetGameObject().Components.Create<FixedJoint>();
		fixedJoint2.Body = thruster;

	}
}
