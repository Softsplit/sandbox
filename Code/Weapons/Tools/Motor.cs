using Softsplit.UI;

namespace Softsplit;

public sealed class Motor : ToolComponent
{
	GameObject object1;
	HighlightOutline object1Outline;
	Vector3 point1Direction;
	Vector3 point1;
   	MotorMenu motorMenu;
	protected override void Start()
	{
        motorMenu = Components.Get<MotorMenu>(true);
		ToolName = "Motor";
		ToolDes = "Connect objects together that can rotate around an axis with a motor.";
	}

	protected override void Update()
	{
		if ( object1Outline != null )
		{
			object1Outline.Enabled = object1 != null;
			if ( object1 == null ) object1Outline = null;
		}

		object1Outline = object1?.Components.Get<HighlightOutline>( true );
	}

	protected override void PrimaryAction()
	{
		var hit = Trace();

		if ( hit.Hit && hit.GameObject?.Name != "Map" )
		{
			if ( hit.GameObject == object1 || hit.Body == null ) return;

			Recoil( hit.EndPosition );

			Vector3 localPoint = hit.GameObject.Transform.World.PointToLocal( hit.EndPosition );

			if ( object1 == null )
			{
				point1 = localPoint;
				point1Direction = hit.Normal;
				object1 = hit.GameObject;
			}
			else
			{
				GameObject object1G = object1;

				object1G.Transform.Rotation = Rotation.FromToRotation( point1Direction, -hit.Normal ) * object1G.Transform.Rotation;

				Vector3 pointWorld = object1G.Transform.World.PointToWorld( point1 );

				object1G.Transform.Position += hit.EndPosition - pointWorld;

				CreateAxis( PlayerState.Local.Pawn.GameObject, object1, hit.GameObject, hit.EndPosition, hit.Normal, motorMenu.Friction, motorMenu.Speed, motorMenu.ForwardBind, motorMenu.BackwardBind, motorMenu.Toggle, PlayerState.Local.Pawn.Id);

				
			}
		}
	}


	protected override void SecondaryAction()
	{
		var hit = Trace();
		if ( hit.Hit && hit.GameObject?.Name != "Map" )
		{
			RemoveAxis( hit.GameObject );
			Recoil( hit.EndPosition );
		}
	}

	[Broadcast]
	public static void CreateAxis( GameObject player, GameObject object1, GameObject object2, Vector3 point2Pos, Vector3 axis, float friction = 0, float speed = 0, string forward = null, string backward = null, bool toggle = false, Guid localPawn = new Guid())
	{
		AxisContext axisContext1 = object1?.Components.Create<AxisContext>();
        axisContext1.MainAxis = true;

        axisContext1.point = object2.Transform.World.PointToLocal(point2Pos);
        axisContext1.Speed = speed;
		axisContext1.localPawn = localPawn;
        axisContext1.Friction = friction;
		axisContext1.Forward = forward;
		axisContext1.Backward = backward;
		axisContext1.Toggle = toggle;
        axisContext1.axis = axis;

        AxisContext axisContext2 = object2?.Components.Create<AxisContext>();
        axisContext2.connectedObject = axisContext1;
        axisContext2.body = object2?.Components.Get<Rigidbody>()?.PhysicsBody;

        axisContext1.connectedObject = axisContext2;
        axisContext1.body = object1?.Components.Get<Rigidbody>()?.PhysicsBody;

        PlayerPawn owner = player?.Components.Get<PlayerPawn>();
        if (owner == PlayerState.Local?.PlayerPawn)
        {
            PlayerState.Thing thing = new()
            {
                components = new List<Component>
                {
                    axisContext1,
                    axisContext2
                }
            };

            owner.PlayerState?.SpawnedThings?.Add(thing);
        }
	}

	[Broadcast]
	public static void RemoveAxis( GameObject gameObject )
	{
		if ( !Networking.IsHost )
			return;

		IEnumerable<AxisContext> axisContext = gameObject?.Components.GetAll<AxisContext>();

		while ( axisContext.Any() )
		{
			AxisContext axisToRemove = axisContext?.ElementAt( 0 );
			if ( axisToRemove.MainAxis ) axisToRemove?.axisJoint?.Remove();
			else axisToRemove?.axisJoint?.Remove();

			axisToRemove?.connectedObject?.Destroy();
			axisToRemove?.Destroy();
		}
	}
}
