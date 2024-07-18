namespace Softsplit;

public sealed class ToolGunHandler : Component
{
	[ConVar( "tool_current" )] public static string CurrentTool { get; set; }

	[Property, ReadOnly] public Component ActiveToolMenu { get; set; }

	private string lastTool;
	private Component activeTool;

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost )
			return;

		if ( lastTool != CurrentTool )
			UpdateTool();

		lastTool = CurrentTool;
	}

	public void UpdateTool()
	{
		if ( activeTool.IsValid() )
		{
			activeTool.Destroy();
			ActiveToolMenu.Destroy();
		}

		TypeDescription comp = TypeLibrary.GetType( $"{CurrentTool}Menu" );
		if ( comp != null )
			ActiveToolMenu = Components.Create( comp, true );
		else
			ActiveToolMenu = null;

		comp = TypeLibrary.GetType( CurrentTool );
		activeTool = Components.Create( comp, true );
	}
}
