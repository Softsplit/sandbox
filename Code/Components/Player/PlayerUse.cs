public sealed class PlayerUse : Component, PlayerController.IEvents
{
	[RequireComponent] public Player Player { get; set; }

	void PlayerController.IEvents.FailPressing()
	{
		BroadcastFailPressing();
	}

	[Rpc.Broadcast]
	private void BroadcastFailPressing()
	{
		Sound.Play( "player_use_fail", WorldPosition );
	}
}
