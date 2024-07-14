using Sandbox.Events;

namespace Softsplit;

public class Loadout
{
	[KeyProperty] public List<EquipmentResource> Equipment { get; set; }
}

public sealed class DefaultEquipment : Component,
	IGameEventHandler<PlayerSpawnedEvent>
{
	/// <summary>
	/// A weapon set that we'll give the player when they spawn.
	/// </summary>
	[Property] public List<Loadout> Loadouts { get; set; }

	[Property] public bool RefillAmmo { get; set; } = true;
	[Property] public bool LoadoutsEnabled { get; set; } = true;

	public Loadout GetLoadout()
	{
		if ( Loadouts.FirstOrDefault() is { } loadout )
		{
			return loadout;
		}

		return Loadouts.FirstOrDefault();
	}

	public bool Contains( EquipmentResource resource )
	{
		var loadout = GetLoadout();
		if ( loadout is null ) return false;

		return loadout.Equipment.Contains( resource );
	}

	void IGameEventHandler<PlayerSpawnedEvent>.OnGameEvent( PlayerSpawnedEvent eventArgs )
	{
		var loadout = GetLoadout();
		if ( loadout is null )
			return;

		var player = eventArgs.Player;

		if ( LoadoutsEnabled )
		{
			foreach ( var resource in player.PlayerState.Loadout.Equipment )
			{
				if ( !player.Inventory.HasInSlot( resource.Slot ) )
				{
					player.Inventory.Give( resource, false );
				}
			}
		}

		foreach ( var weapon in loadout.Equipment )
		{
			if ( !player.Inventory.HasInSlot( weapon.Slot ) )
				player.Inventory.Give( weapon, false );
		}

		player.Inventory.SwitchToBest();

		if ( RefillAmmo )
		{
			player.Inventory.RefillAmmo();
		}
	}
}
