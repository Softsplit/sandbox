using Softsplit;

public class InventoryBar : Panel
{
	readonly List<InventoryIcon> slots = new();

	public InventoryBar()
	{
		for ( int i = 0; i < 9; i++ )
		{
			var icon = new InventoryIcon( i + 1, this );
			slots.Add( icon );
		}
	}

	public override void Tick()
	{
		base.Tick();

		var player = PlayerState.Viewer?.PlayerPawn;

		for ( int i = 0; i < slots.Count; i++ )
		{
			UpdateIcon( GetSlot( player?.Inventory, i ), slots[i] );
		}
	}

	public Equipment GetSlot( PlayerInventory inventory, int i )
	{
		if ( inventory?.Equipment.Count() <= i ) return null;
		if ( i < 0 ) return null;

		return inventory?.Equipment.ToList()[i];
	}

	private static void UpdateIcon( Equipment equipment, InventoryIcon inventoryIcon )
	{
		var player = equipment?.Owner;

		if ( !equipment.IsValid() )
		{
			inventoryIcon.Clear();
			return;
		}

		inventoryIcon.Equipment = equipment;
		inventoryIcon.Label.Text = equipment.Resource.Name;
		inventoryIcon.SetClass( "active", player?.CurrentEquipment == equipment );
	}
}
