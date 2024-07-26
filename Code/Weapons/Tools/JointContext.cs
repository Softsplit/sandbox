using Sandbox.Physics;

namespace Softsplit;

public abstract class JointContext : Component
{
	[Property] public JointContext connectedObject { get; set; }
	[Property] public PhysicsBody body { get; set; }
}