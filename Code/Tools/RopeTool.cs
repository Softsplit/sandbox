public class JointLine
{
	static public void Create( GameObject a, GameObject b )
	{
		GameObject Line = new();
		Line.Components.Create<LineRenderer>();
		Line.Components.GetInChildrenOrSelf<LineRenderer>().Color = Color.Cyan;
		List<GameObject> points = new()
		{
			a,
			b
		};
		Line.Components.GetInChildrenOrSelf<LineRenderer>().Points = points;
		List<Curve.Frame> frames = new()
		{
			new Curve.Frame( 0, 1 ),
			new Curve.Frame( 1, 1 )
		};
		Line.Components.GetInChildrenOrSelf<LineRenderer>().Width = new Curve( frames );
		Line.NetworkSpawn();
	}
}
public class RopeTool : Tool
{
	GameObject lastObject;
	Vector3 lastObjectOffset;
	static public void CreateRope( GameObject a, GameObject b, float Frequency = 5f, float Damping = 0.6f, bool MinLenghtSet = true )
	{
		SpringJoint joint = a.Components.Create<SpringJoint>();
		joint.Body = b;
		joint.Frequency = Frequency;
		joint.Damping = Damping;
		joint.MinLength = 20f;
		joint.MaxLength = 100f;
		joint.EnableCollision = true;
		if ( MinLenghtSet ) joint.MinLength = b.Transform.Position.Distance( a.Transform.Position ) * 2f;

		joint = b.Components.Create<SpringJoint>();
		joint.Body = a;
		joint.Frequency = Frequency;
		joint.Damping = Damping;
		joint.MinLength = 20f;
		joint.MaxLength = 100f;
		joint.EnableCollision = true;
		if ( MinLenghtSet ) joint.MinLength = b.Transform.Position.Distance( a.Transform.Position ) * 2f;

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

			CreateRope( lastObject, picker, 5f, 0.7f, false );
			lastObject = null;
		}
	}
}
