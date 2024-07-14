namespace Softsplit;

/// <summary>
/// Respawn players after a delay.
/// </summary>
public sealed class PlayerAutoRespawner : Component
{
	[Property, HostSync] public float RespawnDelaySeconds { get; set; } = 3f;

	[Authority]
	protected override void OnFixedUpdate()
	{
		foreach ( var player in GameUtils.AllPlayers )
		{
			if ( player.PlayerPawn.IsValid() && player.PlayerPawn.HealthComponent.State == LifeState.Alive )
				continue;

			if ( !player.IsConnected )
				continue;

			switch ( player.RespawnState )
			{
				case RespawnState.Requested:
					player.RespawnState = RespawnState.Delayed;
					break;

				case RespawnState.Delayed:
					if ( player.TimeSinceRespawnStateChanged > RespawnDelaySeconds )
					{
						player.RespawnState = RespawnState.Immediate;
					}
					break;

				case RespawnState.Immediate:
					player.Respawn( true );
					break;
			}
		}
	}
}
