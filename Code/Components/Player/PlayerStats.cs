/// <summary>
/// Record stats for the local player
/// </summary>
public sealed class PlayerStats : Component, Component.INetworkListener, IPlayerEvent
{
	[RequireComponent] public Player Player { get; set; }

	RealTimeUntil timeUntilHalfMarathon = 14400;
	RealTimeUntil timeUntilMarathon = 28800;

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( timeUntilHalfMarathon )
			Sandbox.Services.Achievements.Unlock( "half_marathon" );

		if ( timeUntilMarathon )
			Sandbox.Services.Achievements.Unlock( "marathon" );
	}

	void IPlayerEvent.OnSpawned()
	{
		Sandbox.Services.Stats.Increment( "respawn", 1 );
	}

	void INetworkListener.OnActive( Connection channel )
	{
		if ( channel.SteamId == 76561197960279927 ) Sandbox.Services.Achievements.Unlock( "play_with_garry" );
	}
}
