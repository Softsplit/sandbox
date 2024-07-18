using Sandbox.Events;
using Sandbox.Physics;

namespace Softsplit;

public sealed class Weld : ToolComponent
{
    PhysicsBody object1;
    PhysicsBody object2;
    Vector3 point1;
    Vector3 point2;

    bool advancedWeld;
	protected override void OnFixedUpdate()
	{
		if(advancedWeld)
        {
            object2.MotionEnabled = false;
            object2.GetGameObject().Transform.Position = object1.GetGameObject().Transform.World.PointToWorld(point1) + -(object2.GetGameObject().Transform.World.PointToWorld(point2)-object2.GetGameObject().Transform.Position);
        }
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
                object1 = hit.Body;
            }
            else
            {
                CreateWeld(object1, point1, hit.Body, localPoint);
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
                RemoveWeld(hit.GameObject);
                Recoil(hit.EndPosition);
            }
            else
            {
                object2 = hit.Body;
                advancedWeld = true;
            }
        }
	}

    [Broadcast]
    public static void CreateWeld(PhysicsBody object1, Vector3 point1Pos, PhysicsBody object2, Vector3 point2Pos)
    {
        if ( !Networking.IsHost )
			return;

        WeldContext weldContext1 = object1.GetGameObject().Components.Create<WeldContext>();
        
        weldContext1.MainWeld = true;

        var point1 = new PhysicsPoint( object1, point1Pos );
		var point2 = new PhysicsPoint( object2, point2Pos );
        
        weldContext1.weldJoint = PhysicsJoint.CreateFixed(point1,point2);

        WeldContext weldContext2 = object2.GetGameObject().Components.Create<WeldContext>();
        weldContext2.weldedObject = weldContext1;
        weldContext1.weldedObject = weldContext2;
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
