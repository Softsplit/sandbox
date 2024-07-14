using Sandbox.Events;

namespace Softsplit;

public partial class PlayerRagdollBehavior : Component, IGameEventHandler<OnPlayerRagdolledEvent>
{
	/// <summary>
	/// If set to 0, the player ragdoll will only remove after cleanup.
	/// </summary>
	[Property] public float DestroyTime { get; set; } = 0f;

	void IGameEventHandler<OnPlayerRagdolledEvent>.OnGameEvent( OnPlayerRagdolledEvent eventArgs )
	{
		eventArgs.DestroyTime = DestroyTime;
	}
}
