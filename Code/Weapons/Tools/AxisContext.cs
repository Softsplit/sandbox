using Sandbox.Physics;

namespace Softsplit;

public sealed class AxisContext : JointContext
{
	public PhysicsJoint axisJoint;
	[Property] public bool MainAxis { get; set; }
	[Property] public Vector3 point { get; set; }
	[Property] public Vector3 axis { get; set; }

	protected override void OnStart()
	{
		if ( connectedObject == null )
			Destroy();

		if ( MainAxis )
		{
			if ( Components.Get<Rigidbody>() == null )
				return;

			body = Components.Get<Rigidbody>()?.PhysicsBody;

			if ( body == null ) return;

			if ( connectedObject?.Components.Get<Rigidbody>() == null )
				return;

			connectedObject.body = connectedObject?.Components.Get<Rigidbody>()?.PhysicsBody;

			if ( connectedObject?.body == null ) return;

			if ( body.Equals( connectedObject.body ) )
				return;

			axisJoint = PhysicsJoint.CreateHinge( body, connectedObject.body, point, axis);
		}
	}

	public static Vector3 FindMidpoint( Vector3 vector1, Vector3 vector2 )
	{
		return new Vector3(
			(vector1.x + vector2.x) / 2,
			(vector1.y + vector2.z) / 2,
			(vector1.y + vector2.z) / 2
		);
	}

	protected override void OnDestroy()
	{
		if ( MainAxis )
			axisJoint?.Remove();
	}
}
