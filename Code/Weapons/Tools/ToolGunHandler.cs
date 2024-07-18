using Sandbox.Events;

namespace Softsplit;


public sealed class ToolGunHandler : Component
{
	[ConVar ( "tool_current" )] 
	public static string CurrentTool {get;set;}
	string lastTool;

	Component ActiveTool;
	[Property] public Component ActiveToolMenu;
	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost )
			return;
		
		if(lastTool!=CurrentTool)
		{
			UpdateTool();
		}
		lastTool = CurrentTool;
	}


	public void UpdateTool()
	{
		if(ActiveTool != null)
		{
			ActiveTool.Destroy();
			ActiveToolMenu.Destroy();
		}

		TypeDescription comp = TypeLibrary.GetType($"{CurrentTool}Menu");
		if(comp != null) ActiveToolMenu = Components.Create(comp,true);
		else ActiveToolMenu = null;
		ActiveToolMenu.Enabled = false;

		comp = TypeLibrary.GetType(CurrentTool);
		ActiveTool = Components.Create(comp,true);
		
		

	}
}
