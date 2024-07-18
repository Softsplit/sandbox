using Sandbox.Events;
using Sandbox.Physics;

namespace Softsplit;

public sealed class Weld : ToolComponent
{
    PhysicsBody object1;
	
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

            if(object1 == null) object1 = hit.Body;
            else CreateWeld(object1, hit.Body);

        }
	}

    protected override void SecondaryAction()
	{
        var hit = Scene.Trace.Ray( WeaponRay.Position, WeaponRay.Position+WeaponRay.Forward*500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll", "movement", "player_clip" )
			.Run();
        if(hit.Hit)
        {
            WeldContext weldContext = hit.GameObject.Components.Get<WeldContext>();
            if(weldContext!=null) RemoveWeld(weldContext);
        }
	}

    [Broadcast]
    public void CreateWeld(PhysicsBody object1, PhysicsBody object2)
    {
        if ( !Networking.IsHost )
			return;

        WeldContext weldContext1 = object1.GetGameObject().Components.Create<WeldContext>();
        
        weldContext1.MainWeld = true;

        var point1 = new PhysicsPoint( object1, Vector3.Zero );
		var point2 = new PhysicsPoint( object2, Vector3.Zero );
        
        weldContext1.weldJoint = PhysicsJoint.CreateFixed(point1,point2);

        WeldContext weldContext2 = object2.GetGameObject().Components.Create<WeldContext>();
        weldContext2.weldedObject = weldContext1;
        weldContext1.weldedObject = weldContext2;


    }

    [Broadcast]
    public void RemoveWeld(WeldContext weldToRemove)
    {
        if(weldToRemove.MainWeld) weldToRemove.weldJoint.Remove();
        else weldToRemove.weldedObject.weldJoint.Remove();

        weldToRemove.weldedObject.Destroy();
        weldToRemove.Destroy();
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
