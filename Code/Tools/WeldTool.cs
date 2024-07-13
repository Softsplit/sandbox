public class WeldTool : Tool
{
	GameObject lastObject;
	Vector3 lastObjectOffset;
	static public void CreateWeld( GameObject a, GameObject b, float Frequency = 5f, float Damping = 0.6f, bool MinLenghtSet = true )
	{
		FixedJoint joint = a.Components.Create<FixedJoint>();
		joint.Body = b;

		joint = b.Components.Create<FixedJoint>();
		joint.Body = a;

		// a.NetworkSpawn();
		// b.NetworkSpawn();

		JointLine.Create( b, a );
	}
	protected override void OnUpdate()
	{
		if ( !IsUsing() )
			return;
		if ( !Input.Pressed( "attack1" ) )
			return;

		var aim = DoTrace();
		GameObject picker = aim.GameObject;
		if ( picker == null )
			return;
		if ( aim.Body.BodyType == PhysicsBodyType.Static )
			return;
		if ( lastObject == null )
		{
			lastObject = picker;
			lastObjectOffset = picker.Transform.Position - aim.HitPosition;
		}
		else
		{
			/*GameObject a = new GameObject();
			a.Transform.Position = lastObject.Transform.Position + lastObjectOffset;
			a.Components.Create<FixedJoint>().Body = lastObject;
			a.Components.Create<Rigidbody>();
			lastObject.Components.Create<FixedJoint>().Body = a;

			GameObject b = new GameObject();
			b.Transform.Position = aim.HitPosition;
			b.Components.Create<FixedJoint>().Body = picker;
			b.Components.Create<Rigidbody>();
			picker.Components.Create<FixedJoint>().Body = b;*/

			CreateWeld( lastObject, picker, 5f, 0.7f, false );
			lastObject = null;
		}
	}
}
