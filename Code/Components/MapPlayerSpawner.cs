public sealed class MapPlayerSpawner : Component
{
	protected override void OnAwake()
	{
		// NOTE: This is mostly a workaround because I can't be bothered
		// to check if it's the game scene or not.
		Networking.CreateLobby();
	}
}
