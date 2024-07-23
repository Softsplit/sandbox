namespace Softsplit;

public sealed class ToolGunHandler : Component
{
	[ConVar( "tool_current" )] public static string CurrentTool { get; set; } = "Scale";

	public Component ActiveToolMenu { get; set; }
	public ToolComponent ActiveTool { get; set; }

	private string lastTool;

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( lastTool != CurrentTool && lastTool != "" )
			UpdateTool();

		if ( lastTool != "" )
			lastTool = CurrentTool;
	}

	public void UpdateTool()
	{
		ActiveTool?.Destroy();
		ActiveToolMenu?.Destroy();

		

		var comp = TypeLibrary.GetType( $"{CurrentTool}Menu" );
		if ( comp != null )
			ActiveToolMenu = Components.Create( comp, false );
		else
			ActiveToolMenu = null;

		comp = TypeLibrary.GetType( CurrentTool );
		Components.Create( comp, true );
		
		ActiveTool = Components.Get<ToolComponent>();
	}
}
