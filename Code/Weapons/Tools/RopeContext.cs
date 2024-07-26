using Sandbox.Physics;

namespace Softsplit;

public sealed class RopeContext : JointContext
{
	public PhysicsJoint ropeJoint;
	[Property] public bool MainRope { get; set; }
	[Property] public Vector3 point1 { get; set; }
	[Property] public Vector3 point2 { get; set; }

	protected override void OnStart()
	{
		if ( connectedObject == null )
			Destroy();

		if ( MainRope )
		{
			if ( Components.Get<Rigidbody>() == null )
				return;

			body = Components.Get<Rigidbody>()?.PhysicsBody;

			if ( body == null ) return;

			var p1 = new PhysicsPoint( body, point1 );

			if ( connectedObject?.Components.Get<Rigidbody>() == null )
				return;

			connectedObject.body = connectedObject?.Components.Get<Rigidbody>()?.PhysicsBody;

			if ( connectedObject?.body == null ) return;

			var p2 = new PhysicsPoint( connectedObject?.body, point2 );

			if ( body.Equals( connectedObject.body ) )
				return;

			ropeJoint = PhysicsJoint.CreateSpring( p1, p2, 20, 200 );
		}
	}

	protected override void OnDestroy()
	{
		if ( MainRope )
			ropeJoint?.Remove();
	}
}
