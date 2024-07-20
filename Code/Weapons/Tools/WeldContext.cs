using Sandbox.Physics;

namespace Softsplit;

public sealed class WeldContext : Component
{
    [Property] public WeldContext weldedObject {get;set;}
    public PhysicsJoint weldJoint;
    [Property] public PhysicsBody body {get;set;}
	[Property] public bool MainWeld {get;set;}
	[Property] public Vector3 point1 {get;set;}
	[Property] public Vector3 point2 {get;set;}

	protected override void OnStart()
	{
        if(weldedObject == null) Destroy();
		if(MainWeld)
        {
            if(body == null) body = Components.Get<Rigidbody>().PhysicsBody;
            var p1 = new PhysicsPoint( body, point1 );

            if(weldedObject.body == null) weldedObject.body = weldedObject.Components.Get<Rigidbody>().PhysicsBody;
            var p2 = new PhysicsPoint( weldedObject.body, point2 );
            
            weldJoint = PhysicsJoint.CreateFixed(p1,p2);
        }
	}
    protected override void OnDestroy()
	{
		if(MainWeld) weldJoint.Remove();
	}
}