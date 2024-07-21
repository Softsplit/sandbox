using Sandbox.Physics;

namespace Softsplit;

public sealed class WeldContext : Component
{
	[Property] public WeldContext weldedObject { get; set; }
	public PhysicsJoint weldJoint;
	[Property] public PhysicsBody body { get; set; }
	[Property] public bool MainWeld { get; set; }
	[Property] public Vector3 point1 { get; set; }
	[Property] public Vector3 point2 { get; set; }

	protected override void OnStart()
	{
		if ( weldedObject == null )
			Destroy();

		if ( MainWeld )
		{
			if ( Components.Get<Rigidbody>() == null )
				return;

			body = Components.Get<Rigidbody>()?.PhysicsBody;

			if ( body == null ) return;

			var p1 = new PhysicsPoint( body, point1 );

			if ( weldedObject?.Components.Get<Rigidbody>() == null )
				return;

			weldedObject.body = weldedObject?.Components.Get<Rigidbody>()?.PhysicsBody;

			if ( weldedObject?.body == null ) return;

			var p2 = new PhysicsPoint( weldedObject?.body, point2 );

			if ( body.Equals( weldedObject.body ) )
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
