/// <summary>
/// Record stats for the local player
/// </summary>
public sealed class PlayerStats : Component, Component.INetworkListener, IPlayerEvent
{
	[RequireComponent] public PlayerController PlayerController { get; set; }
	[RequireComponent] public Player Player { get; set; }

	void IPlayerEvent.OnSpawned( Player player )
	{
		if ( player != Player )
			return;

		Sandbox.Services.Stats.Increment( "respawn", 1 );
	}

	void INetworkListener.OnActive( Connection channel )
	{
		if ( IsProxy )
			return;

		if ( channel.SteamId == 76561197960279927 )
			Sandbox.Services.Achievements.Unlock( "play_with_garry" );
	}
}
