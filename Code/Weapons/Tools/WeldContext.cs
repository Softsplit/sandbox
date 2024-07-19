using Sandbox.Events;

namespace Softsplit;

public sealed class WeldContext : Component
{
    public WeldContext weldedObject;
    public Sandbox.Physics.PhysicsJoint weldJoint;
    public PhysicsBody body;
	public bool MainWeld;
}