using Sandbox.Events;

namespace Softsplit;

public sealed class RopeContext : Component
{
    public RopeContext weldedObject;
    public Sandbox.Physics.SpringJoint weldJoint;
	public bool MainWeld;
}
