public sealed class PlayerUse : Component, PlayerController.IEvents
{
	[RequireComponent] public Player Player { get; set; }

	void PlayerController.IEvents.FailPressing()
	{
		BroadcastFailPressing();
	}

	[Broadcast]
	private void BroadcastFailPressing()
	{
		Sound.Play( "player_use_fail", WorldPosition );
	}
}
