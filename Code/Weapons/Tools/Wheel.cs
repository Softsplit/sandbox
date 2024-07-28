using Softsplit.UI;

namespace Softsplit;

public sealed class Wheel : ToolComponent
{
	string WheelModel = "models/citizen_props/wheel01.vmdl";
	HighlightOutline object1Outline;
   	WheelMenu wheelMenu;
	protected override void Start()
	{
        wheelMenu = Components.Get<WheelMenu>(true);
		ToolName = "Wheel";
		ToolDes = "Spawn a wheelised wheel on an object.";
	}

	protected override void Update()
	{

	}

	protected override void PrimaryAction()
	{
		var hit = Trace();

		if ( hit.Hit && hit.GameObject?.Name != "Map" )
		{
			GameObject object1G = new GameObject();
			object1G.Transform.Position = 0;
			object1G.Transform.Rotation = Angles.Zero;
			ModelCollider modelCollider = object1G.Components.Create<ModelCollider>();
			modelCollider.Model = Model.Load(WheelModel);
			object1G.Components.Create<ModelRenderer>().Model = Model.Load(WheelModel);
			object1G.Components.Create<Rigidbody>();

			Vector3 point1Direction = Vector3.Zero.WithY(-1);
			Vector3 point1 = Vector3.Zero.WithY((-modelCollider.Model.Bounds.Size.y/2)-1.5f);

			object1G.Transform.Rotation = Rotation.FromToRotation( point1Direction, -hit.Normal ) * object1G.Transform.Rotation;

			Vector3 pointWorld = object1G.Transform.World.PointToWorld( point1 );

			object1G.Transform.Position += hit.EndPosition - pointWorld;

			CreateAxis( PlayerState.Local.Pawn.GameObject, object1G, hit.GameObject, hit.EndPosition, hit.Normal, wheelMenu.Friction, wheelMenu.Speed, wheelMenu.ForwardBind, wheelMenu.BackwardBind, wheelMenu.Toggle, PlayerState.Local.Pawn.Id);

			object1G.NetworkSpawn();
			PlayerState.Thing thing = new()
			{
				gameObjects = new List<GameObject>{object1G}
			};

			PlayerState.Local?.SpawnedThings.Add(thing);
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
                },
				gameObjects = new List<GameObject>{object1}
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
