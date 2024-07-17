using Sandbox.UI.Construct;
using Softsplit;

public class InventoryIcon : Panel
{
	public Equipment Equipment;
	public Label Label;
	public Label Number;

	public InventoryIcon( int i, Panel parent )
	{
		Parent = parent;
		Label = Add.Label( "empty", "item-name" );
		Number = Add.Label( $"{i}", "slot-number" );
	}

	public void Clear()
	{
		Label.Text = "";
		SetClass( "active", false );
	}
}
