namespace Softsplit;

public sealed class ToolGunHandler : Component
{
	[ConVar( "tool_current" )] public static string CurrentTool { get; set; } = "TestTool";

	public Component ActiveToolMenu { get; set; }
	public ToolGunUI ToolGunUI { get; set; }

	protected override void OnStart()
	{
		ToolGunUI = Components.GetOrCreate<ToolGunUI>();
	}

	private string lastTool;
	private Component activeTool;

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( lastTool != CurrentTool && lastTool != "" )
			UpdateTool();

		if ( lastTool != "" )
			lastTool = CurrentTool;
	}

	public void UpdateTool()
	{
		activeTool?.Destroy();
		ActiveToolMenu?.Destroy();

		var comp = TypeLibrary.GetType( $"{CurrentTool}Menu" );
		if ( comp != null )
			ActiveToolMenu = Components.Create( comp, true );
		else
			ActiveToolMenu = null;

		comp = TypeLibrary.GetType( CurrentTool );
		activeTool = Components.Create( comp, true );
	}
}
