using Softsplit;

public sealed class ToolName : Component
{
	TextRenderer textRenderer;

	protected override void OnStart()
	{
		textRenderer = Components.Get<TextRenderer>();
	}

	protected override void OnUpdate()
	{
		textRenderer.Text = GetCurrentTool()?.ToolName.Substring(0,4).ToUpper();
	}

	ToolComponent GetCurrentTool()
	{
		var player = PlayerState.Viewer?.PlayerPawn;
		if ( player?.CurrentEquipment?.Resource.Name != "Toolgun" )
			return null;

		return player?.Components.Get<ToolGunHandler>( FindMode.EverythingInSelfAndDescendants )?.ActiveTool;
	}
}
