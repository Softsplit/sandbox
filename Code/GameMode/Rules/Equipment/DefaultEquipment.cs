using Sandbox.Events;

namespace Softsplit;

public sealed class DefaultEquipment : Component,
	IGameEventHandler<PlayerSpawnedEvent>
{
	/// <summary>
	/// A list of equipment resources that we'll give the player when they spawn.
	/// </summary>
	[Property] public List<EquipmentResource> Equipment { get; set; }

	[Property] public bool RefillAmmo { get; set; } = true;

	public bool Contains( EquipmentResource resource )
	{
		return Equipment != null && Equipment.Contains( resource );
	}

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		var player = eventArgs.Player;

		if ( Equipment != null )
		{
			foreach ( var weapon in Equipment )
			{
				if ( !player.Inventory.Has( weapon ) )
					player.Inventory.Give( weapon, false );
			}
		}

		player.Inventory.SwitchToBest();

		if ( RefillAmmo )
		{
			player.Inventory.RefillAmmo();
		}
	}
}
