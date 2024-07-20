using Sandbox.Events;
using Sandbox.Physics;

namespace Softsplit;

public sealed class Weld : ToolComponent
{
    PhysicsBody object1;
    Vector3 point1Direction;
    Vector3 point1;

	protected override void Start()
	{
        ToolName = "Weld";
        ToolDes = "Weld objects together. Right click to snap.";        
	}
	protected override void PrimaryAction()
	{
        var hit = Trace();
        if(hit.Hit  && hit.GameObject.Name != "Map")
        {
            if(hit.Body == object1 || hit.Body == null) return;
            Recoil(hit.EndPosition);
            Vector3 localPoint = hit.GameObject.Transform.World.PointToLocal(hit.EndPosition);
            if(object1 == null)
            {
                point1 = localPoint;
                point1Direction = hit.Normal;
                object1 = hit.Body;
            }
            else
            {
                CreateWeld(PlayerState.Local.PlayerPawn, object1, point1, hit.Body, localPoint);
                object1 = null;
            }
            
        }
	}


    protected override void SecondaryAction()
	{

        base.SecondaryAction();
        var hit = Trace();
        if(hit.Hit  && hit.GameObject.Name != "Map")
        {
            if(object1 == null)
            {
                RemoveWeld(hit.GameObject);
                Recoil(hit.EndPosition);
            }
            else
            {
                GameObject object1G = object1.GetGameObject();

                object1G.Transform.Rotation = Rotation.FromToRotation(point1Direction, -hit.Normal) * object1G.Transform.Rotation;
                
                Vector3 pointWorld = object1G.Transform.World.PointToWorld(point1);

                object1G.Transform.Position +=  hit.EndPosition - pointWorld;

                CreateWeld(PlayerState.Local.PlayerPawn, object1, point1, hit.Body, hit.EndPosition);
                object1 = null;
            }
        }
	}

    [Broadcast]
    public static void CreateWeld(PlayerPawn owner, PhysicsBody object1, Vector3 point1Pos, PhysicsBody object2, Vector3 point2Pos)
    {
        if ( !Networking.IsHost )
			return;

        WeldContext weldContext1 = object1.GetGameObject().Components.Create<WeldContext>();
        weldContext1.MainWeld = true;

        weldContext1.point1 = point1Pos;
        weldContext1.point2 = point2Pos;

        WeldContext weldContext2 = object2.GetGameObject().Components.Create<WeldContext>();
        weldContext2.weldedObject = weldContext1;
        weldContext2.body = object2;

        weldContext1.weldedObject = weldContext2;
        weldContext1.body = object1;

        if(owner == PlayerState.Local.PlayerPawn)
        {
            PlayerState.Thing thing = new PlayerState.Thing{
                component = weldContext1
            };
            owner.PlayerState.SpawnedThings.Add(thing);

            thing = new PlayerState.Thing{
                component = weldContext2
            };
            owner.PlayerState.SpawnedThings.Add(thing);
        }
    }

    [Broadcast]
    public static void RemoveWeld(GameObject gameObject)
    {
        if ( !Networking.IsHost )
			return;
        
        IEnumerable<WeldContext> weldContext = gameObject.Components.GetAll<WeldContext>();

        while ( weldContext.Any())
        {
            WeldContext weldToRemove = weldContext.ElementAt(0);
            if(weldToRemove.MainWeld) weldToRemove.weldJoint.Remove();
            else weldToRemove.weldedObject.weldJoint.Remove();

            weldToRemove.weldedObject.Destroy();
            weldToRemove.Destroy();
        }
        
    }

    public static Vector3 FindMidpoint(Vector3 vector1, Vector3 vector2)
    {
        return new Vector3(
            (vector1.x + vector2.x) / 2,
            (vector1.y + vector2.z) / 2,
            (vector1.y + vector2.z) / 2
        );
    }
}
