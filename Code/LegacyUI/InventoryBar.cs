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
		var player = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();
		if ( player == null ) return;
		var inventory = player.Inventory;
		if ( inventory == null ) return;

		for ( int i = 0; i < slots.Count; i++ )
		{
			UpdateIcon( inventory.GetSlot( i ), slots[i], i );
		}

		ProcessClientInput();
	}

	private static void UpdateIcon( Tool ent, InventoryIcon inventoryIcon, int i )
	{
		var player = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();

		if ( ent == null )
		{
			inventoryIcon.Clear();
			return;
		}

		var di = DisplayInfo.For( ent );

		inventoryIcon.TargetEnt = ent.GameObject;
		inventoryIcon.Label.Text = di.Name;
		inventoryIcon.SetClass( "active", player.ActiveChild == ent );
	}

	public void ProcessClientInput()
	{
		var player = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();
		if ( player == null ) return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;
		/*
		if ( player.ActiveChild is PhysGun physgun && physgun.BeamActive )
		{
			return;
		}*/

		if ( Input.Pressed( "slot1" ) ) SetActiveSlot( inventory, 0 );
		if ( Input.Pressed( "slot2" ) ) SetActiveSlot( inventory, 1 );
		if ( Input.Pressed( "slot3" ) ) SetActiveSlot( inventory, 2 );
		if ( Input.Pressed( "slot4" ) ) SetActiveSlot( inventory, 3 );
		if ( Input.Pressed( "slot5" ) ) SetActiveSlot( inventory, 4 );
		if ( Input.Pressed( "slot6" ) ) SetActiveSlot( inventory, 5 );
		if ( Input.Pressed( "slot7" ) ) SetActiveSlot( inventory, 6 );
		if ( Input.Pressed( "slot8" ) ) SetActiveSlot( inventory, 7 );
		if ( Input.Pressed( "slot9" ) ) SetActiveSlot( inventory, 8 );

		if ( Input.MouseWheel != 0 ) SwitchActiveSlot( inventory, (int)-Input.MouseWheel.y );
	}

	private static void SetActiveSlot( Inventory inventory, int i )
	{
		var player = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();

		if ( player == null ) return;

		var ent = inventory.GetSlot( i );
		if ( player.ActiveChild == ent )
			return;

		if ( ent == null )
			return;

		player.ActiveChild = ent;
	}

	private static void SwitchActiveSlot( Inventory inventory, int idelta )
	{
		var count = inventory.Count();
		if ( count == 0 ) return;

		var slot = inventory.GetActiveSlot();
		var nextSlot = slot + idelta;

		while ( nextSlot < 0 ) nextSlot += count;
		while ( nextSlot >= count ) nextSlot -= count;

		SetActiveSlot( inventory, nextSlot );
	}
}
