using Sandbox.UI;
using Sandbox.UI.Construct;

public class CurrentTool : Panel
{
	public Label Title;
	public Label Description;

	public CurrentTool()
	{
		Title = Add.Label( "WORK IN PROGRESS!", "title" );
		Description = Add.Label( "Join the Discord in the meantime: https://discord.gg/rbCJdZjewf", "description" );
	}

	/*
	public override void Tick()
	{
		var tool = GetCurrentTool();
		SetClass( "active", tool != null );

		if ( tool != null )
		{
			var display = DisplayInfo.For( tool );

			Title.SetText( display.Name );
			Description.SetText( display.Description );
		}
	}

	BaseTool GetCurrentTool()
	{
		var player = Game.LocalPawn as Player;
		if ( player == null ) return null;

		var inventory = player.Inventory;
		if ( inventory == null ) return null;

		if ( inventory.Active is not Tool tool ) return null;

		return tool?.CurrentTool;
	}
	*/
}
