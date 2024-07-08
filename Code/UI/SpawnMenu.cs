using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

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

				var npclist = body.AddChild<NpcList>();
				tabs.AddButtonActive( "#spawnmenu.npclist", ( b ) => npclist.SetClass( "active", b ) );

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
		var player = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();
		if ( player == null ) return;
		var inventory = player.Inventory;
		if ( inventory == null ) return;

		foreach ( var entry in TypeLibrary.GetTypes<Tool>() )
		{
			if ( entry.Name == "Tool" )
				continue;

			var button = toollist.Add.Button( entry.Title );
			if ( player.ActiveChild != null )
			{
				button.SetClass( "active", entry.ClassName == player.ActiveChild.GetType().Name );
			}

			button.AddEventListener( "onclick", () =>
			{
				SetActiveTool( entry, inventory, player );

				foreach ( var child in toollist.Children )
					child.SetClass( "active", child == button );
			} );
		}
	}

	void SetActiveTool( TypeDescription classtype, Inventory inventory, Player player )
	{
		var obj = new GameObject();
		var tool = obj.Components.Create( classtype );
		obj.NetworkSpawn();
		inventory.Add( tool as Tool );

		// set the active weapon to the toolgun
		/*
		if ( Game.LocalPawn is not Player player ) return;
		if ( player.Inventory is null ) return;

		// why isn't inventory just an ienumurable wtf
		for ( int i = 0; i < player.Inventory.Count(); i++ )
		{
			var entity = player.Inventory.GetSlot( i );
			if ( !entity.IsValid() ) continue;
			if ( entity.ClassName != "weapon_tool" ) continue;

			player.ActiveChildInput = entity;
		}
		*/
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
		// var tool = string.IsNullOrWhiteSpace( toolCurrent ) ? null : TypeLibrary.GetType<BaseTool>( toolCurrent );

		foreach ( var child in toollist.Children )
		{
			if ( child is Button button )
			{
				// child.SetClass( "active", tool != null && button.Text == tool.Title );
			}
		}
	}

	public override void OnHotloaded()
	{
		RebuildToolList();
	}
}
