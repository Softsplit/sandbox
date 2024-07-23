using Sandbox.Physics;

namespace Softsplit;

public sealed class RopeContext : Component
{
	[Property] public RopeContext ropededObject { get; set; }
	public PhysicsJoint ropeJoint;
	[Property] public PhysicsBody body { get; set; }
	[Property] public bool MainRope { get; set; }
	[Property] public Vector3 point1 { get; set; }
	[Property] public Vector3 point2 { get; set; }

	protected override void OnStart()
	{
		if ( ropededObject == null )
			Destroy();

		if ( MainRope )
		{
			if ( Components.Get<Rigidbody>() == null )
				return;

			body = Components.Get<Rigidbody>()?.PhysicsBody;

			if ( body == null ) return;

			var p1 = new PhysicsPoint( body, point1 );

			if ( ropededObject?.Components.Get<Rigidbody>() == null )
				return;

			ropededObject.body = ropededObject?.Components.Get<Rigidbody>()?.PhysicsBody;

			if ( ropededObject?.body == null ) return;

			var p2 = new PhysicsPoint( ropededObject?.body, point2 );

			if ( body.Equals( ropededObject.body ) )
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
