using Sandbox.UI.Construct;
using Softsplit;

public class CurrentTool : Panel
{
	public Label Title;
	public Label Description;

	public CurrentTool()
	{
		Title = Add.Label( "#hud.gamestatus.title", "title" );
		Description = Add.Label( "#hud.gamestatus.description", "description" );
	}

	public override void Tick()
	{
		var tool = GetCurrentTool();
		SetClass( "active", tool != null );

		if ( tool != null )
		{
			Title.Text = tool.ToolName;
			Description.Text = tool.ToolDes;
		}
	}

	ToolComponent GetCurrentTool()
	{
		var player = PlayerState.Viewer?.PlayerPawn;
		if ( player?.CurrentEquipment.Resource.Name != "Toolgun" )
			return null;

		return (ToolComponent)(player?.Components.Get<ToolGunHandler>( FindMode.EverythingInSelfAndDescendants )?.ActiveTool);
	}
}
