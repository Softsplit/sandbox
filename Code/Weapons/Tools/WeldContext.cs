using Sandbox.Physics;

namespace Softsplit;

public sealed class WeldContext : Component
{
    [Property] public WeldContext weldedObject {get;set;}
    [Property] public Sandbox.Physics.PhysicsJoint weldJoint {get;set;}
    [Property] public PhysicsBody body {get;set;}
	[Property] public bool MainWeld {get;set;}
	[Property] public Vector3 point1 {get;set;}
	[Property] public Vector3 point2 {get;set;}

	protected override void OnStart()
	{
        body = Components.Get<Rigidbody>().PhysicsBody;
		if(MainWeld)
        {
            var p1 = new PhysicsPoint( body, point1 );
            var p2 = new PhysicsPoint( weldedObject.body, point2 );
            
            weldJoint = PhysicsJoint.CreateFixed(p1,p2);
        }
	}
}