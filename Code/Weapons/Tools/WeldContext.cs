using Sandbox.Events;

namespace Softsplit;

public sealed class WeldContext : Component
{
    public WeldContext weldedObject;
    public Sandbox.Physics.FixedJoint weldJoint;
	public bool MainWeld;
}