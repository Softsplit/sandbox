using Sandbox.Physics;

namespace Softsplit;

public sealed class AxisContext : JointContext
{
	public Sandbox.Physics.HingeJoint axisJoint;
	[Property] public bool MainAxis { get; set; }
	[Property] public float Speed { get; set; }
	[Property] public float Friction { get; set; }
	[Property] public string Forward { get; set; }
	[Property] public string Backward { get; set; }
	[Property] public bool Toggle { get; set; }
	[Property] public Guid localPawn { get; set; }
	[Property] public Vector3 point { get; set; }
	[Property] public Vector3 axis { get; set; }

	[Sync] public bool on {get;set;}
	[Sync] public bool direction {get;set;}

	void Inputs()
	{
		
		if(Toggle)
		{
			if(Input.Pressed(Forward))
			{
				on = !on;
				direction = true;
			}
			if(Input.Pressed(Backward))
			{
				on = !on;
				direction = false;
			}
		}
		else
		{
			if(Input.Down(Forward))
			{
				on = true;
				direction = true;
			}
			else if(Input.Down(Backward))
			{
				on = true;
				direction = false;
			}
			else
			{
				on = false;
			}
		}
	}

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

			axisJoint = PhysicsJoint.CreateHinge( body, connectedObject.body, connectedObject.Transform.World.PointToWorld(point), axis);
			axisJoint.Friction = Friction;
		}
	}

	protected override void OnUpdate()
	{
		if(PlayerState.Local.Pawn.Id == localPawn && MainAxis)
		{
			if(Network.OwnerId != localPawn) foreach (GameObject g in PhysGunComponent.GetAllConnectedWelds(GameObject)) g.Network.TakeOwnership();
		}
		else return;

		Inputs();
		
		if(on)
		{
			body.AngularVelocity = axisJoint.Axis * (direction ? Speed : -Speed);
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
