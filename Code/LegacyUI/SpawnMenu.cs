using Sandbox.UI.Construct;
using Softsplit;

public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;
	readonly Panel toollist;

	private static ModelList modelList;
	private bool isSearching;

	public SpawnMenu()
	{
		Instance = this;

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );

			var body = left.Add.Panel( "body" );
			{
				modelList = body.AddChild<ModelList>();
				tabs.SelectedButton = tabs.AddButtonActive( "#spawnmenu.modellist", ( b ) => modelList.SetClass( "active", b ) );

				var ents = body.AddChild<EntityList>();
				tabs.AddButtonActive( "#spawnmenu.entities", ( b ) => ents.SetClass( "active", b ) );

				var npcs = body.AddChild<NpcList>();
				tabs.AddButtonActive( "NPCs", ( b ) => npcs.SetClass( "active", b ) );

				var props = body.AddChild<SpawnList>();
				tabs.AddButtonActive( "#spawnmenu.props", ( b ) => props.SetClass( "active", b ) );
			}
		}

		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				tabs.Add.Button( "#spawnmenu.tools" ).AddClass( "active" );
				tabs.Add.Button( "#spawnmenu.utility" );
			}

			var body = right.Add.Panel( "body" );
			{
				toollist = body.Add.Panel( "toollist" );
				{
					RebuildToolList();
				}
				body.Add.Panel( "inspector" );
			}
		}
	}

	void RebuildToolList()
	{
		toollist.DeleteChildren( true );

		foreach ( var entry in TypeLibrary.GetTypes<ToolComponent>() )
		{
			if ( entry.Name == "ToolComponent" )
				continue;

			var button = toollist.Add.Button( entry.Title );
			button.SetClass( "active", entry.ClassName == ConsoleSystem.GetValue( "tool_current" ) );

			button.AddEventListener( "onclick", () =>
			{
				SetActiveTool( entry.ClassName );

				foreach ( var child in toollist.Children )
					child.SetClass( "active", child == button );
			} );
		}
	}

	void SetActiveTool( string className )
	{
		// setting a cvar
		ConsoleSystem.Run( "tool_current", className );

		var player = PlayerState.Local?.PlayerPawn;

		foreach ( var equipment in player?.Inventory?.Equipment )
		{
			if ( equipment.Resource.Name != "Toolgun" ) continue;

			player?.Inventory?.Switch( equipment );
		}
	}

	public override void Tick()
	{
		if ( modelList.SearchInput.HasFocus )
		{
			isSearching = true;
		}

		if ( isSearching && Input.Pressed( "menu" ) )
		{
			isSearching = false;
		}

		UpdateActiveTool();

		if ( isSearching )
			return;

		Parent.SetClass( "spawnmenuopen", Input.Down( "menu" ) );
	}

	void UpdateActiveTool()
	{
		var toolCurrent = ConsoleSystem.GetValue( "tool_current" );
		var tool = string.IsNullOrWhiteSpace( toolCurrent ) ? null : TypeLibrary.GetType<ToolComponent>( toolCurrent );

		foreach ( var child in toollist.Children )
		{
			if ( child is Button button )
			{
				child.SetClass( "active", tool != null && button.Text == tool.Title );
			}
		}
	}

	public override void OnHotloaded()
	{
		RebuildToolList();
	}
}
