using Sandbox.Physics;

namespace Softsplit;

public sealed class WeldContext : JointContext
{
	public PhysicsJoint weldJoint;
	[Property] public bool MainWeld { get; set; }
	[Property] public Vector3 point1 { get; set; }
	[Property] public Vector3 point2 { get; set; }

	protected override void OnStart()
	{
		if ( connectedObject == null )
			Destroy();

		if ( MainWeld )
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

			weldJoint = PhysicsJoint.CreateFixed( p1, p2 );
		}
	}

	protected override void OnDestroy()
	{
		if ( MainWeld )
			weldJoint?.Remove();
	}
}
