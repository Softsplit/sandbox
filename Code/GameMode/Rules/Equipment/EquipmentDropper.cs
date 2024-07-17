using Sandbox.Events;

namespace Softsplit;

/// <summary>
/// Controls what equipment can be dropped by players, either when killed or with a key bind.
/// </summary>
public partial class EquipmentDropper : Component,
	IGameEventHandler<KillEvent>
{
	void IGameEventHandler<KillEvent>.OnGameEvent( KillEvent eventArgs )
	{
		if ( !Networking.IsHost )
			return;

		var player = GameUtils.GetPlayerFromComponent( eventArgs.DamageInfo.Victim );
		if ( !player.IsValid() )
			return;

		var droppable = player.Inventory.Equipment.ToArray();

		foreach ( var equipment in droppable )
		{
			player.Inventory.Drop( equipment );
		}

		player.Inventory.Clear();
	}
}
