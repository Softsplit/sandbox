using Sandbox.Events;
using Sandbox.Physics;

namespace Softsplit;

public sealed class Rope : ToolComponent
{
    PhysicsBody object1;
    Vector3 point1Direction;
    Vector3 point1;

	protected override void Start()
	{
        ToolName = "Rope";
        ToolDes = "Rope objects together. Right click to snap.";        
	}
	protected override void PrimaryAction()
	{
        var hit = Scene.Trace.Ray( WeaponRay.Position, WeaponRay.Position+WeaponRay.Forward*500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll", "movement", "player_clip" )
			.Run();
        if(hit.Hit)
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
                CreateRope(object1, point1, hit.Body, localPoint);
                object1 = null;
            }
            
        }
	}

    protected override void SecondaryAction()
	{

        base.SecondaryAction();
        var hit = Scene.Trace.Ray( WeaponRay.Position, WeaponRay.Position+WeaponRay.Forward*500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll", "movement", "player_clip" )
			.Run();
        if(hit.Hit)
        {
            if(object1 == null)
            {
                RemoveRope(hit.GameObject);
                Recoil(hit.EndPosition);
            }
            else
            {
                GameObject object1G = object1.GetGameObject();
                GameObject object2G = hit.GameObject;

                object1G.Transform.Rotation = Rotation.FromToRotation(point1Direction, -hit.Normal) * object1G.Transform.Rotation;
                
                Vector3 pointWorld = object1G.Transform.World.PointToWorld(point1);

                object1G.Transform.Position +=  hit.EndPosition - pointWorld;

                CreateRope(object1, point1, hit.Body, hit.EndPosition);
            }
        }
	}

    [Broadcast]
    public static void CreateRope(PhysicsBody object1, Vector3 point1Pos, PhysicsBody object2, Vector3 point2Pos)
    {
        if ( !Networking.IsHost )
			return;

		RopeContext weldContext1 = object1.GetGameObject().Components.Create<RopeContext>();
        
        weldContext1.MainWeld = true;

        var point1 = new PhysicsPoint( object1, point1Pos );
		var point2 = new PhysicsPoint( object2, point2Pos );
        
        weldContext1.weldJoint = PhysicsJoint.CreateSpring(point1,point2, 10f, 100f);

        RopeContext weldContext2 = object2.GetGameObject().Components.Create<RopeContext>();
        weldContext2.weldedObject = weldContext1;
        weldContext1.weldedObject = weldContext2;
    }

    [Broadcast]
    public static void RemoveRope(GameObject gameObject)
    {
        if ( !Networking.IsHost )
			return;
        
        IEnumerable<RopeContext> weldContext = gameObject.Components.GetAll<RopeContext>();

        while ( weldContext.Any())
        {
			RopeContext weldToRemove = weldContext.ElementAt(0);
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
