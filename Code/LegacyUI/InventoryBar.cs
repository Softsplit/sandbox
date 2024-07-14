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

		var player = PlayerState.Local;
		if ( player == null ) return;
		if ( player.PlayerPawn == null ) return;

		for ( int i = 0; i < Math.Min( slots.Count, player.PlayerPawn.Inventory.Equipment.Count() ); i++ )
		{
			UpdateIcon( player.PlayerPawn.Inventory.Equipment.ToList()[i], slots[i], i );
		}
	}

	private static void UpdateIcon( Equipment equipment, InventoryIcon inventoryIcon, int i )
	{
		var player = PlayerState.Local.PlayerPawn;

		if ( equipment == null )
		{
			inventoryIcon.Clear();
			return;
		}

		inventoryIcon.TargetEnt = equipment;
		inventoryIcon.Label.Text = equipment.Resource.Name;
		inventoryIcon.SetClass( "active", player.CurrentEquipment == equipment );
	}
}
