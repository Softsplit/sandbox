using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;
using System.Threading.Tasks;

[Library]
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

		/*
		foreach ( var entry in TypeLibrary.GetTypes<BaseTool>() )
		{
			if ( entry.Name == "BaseTool" )
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
		*/
	}

	void SetActiveTool( string className )
	{
		// setting a cvar
		ConsoleSystem.Run( "tool_current", className );

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

	[ConCmd( "spawn" )]
	public static async Task Spawn( string modelname )
	{
		var owner = Game.ActiveScene.GetAllComponents<PlayerController>().FirstOrDefault().GameObject;

		if ( owner == null )
			return;

		var tr = Game.ActiveScene.Trace.Ray( Game.ActiveScene.Camera.Transform.Position, Game.ActiveScene.Camera.Transform.Position + Game.ActiveScene.Camera.Transform.Rotation.Forward * 500 )
			.UseHitboxes()
			.IgnoreGameObject( owner )
			.Run();

		var modelRotation = Rotation.From( new Angles( 0, Game.ActiveScene.Camera.Transform.Rotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

		//
		// Does this look like a package?
		//
		if ( modelname.Count( x => x == '.' ) == 1 && !modelname.EndsWith( ".vmdl", System.StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", System.StringComparison.OrdinalIgnoreCase ) )
		{
			modelname = await SpawnPackageModel( modelname, tr.EndPosition, modelRotation, owner );
			if ( modelname == null )
				return;
		}

		var model = Model.Load( modelname );
		if ( model == null || model.IsError )
			return;

		var ent = new GameObject();
		ent.Transform.Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z;
		ent.Transform.Rotation = modelRotation;
		ent.Components.Create<Prop>().Model = model;
		ent.NetworkSpawn();
		ent.Network.DropOwnership();

		Sandbox.Services.Stats.Increment( "spawn.model", 1, modelname );
	}

	static async Task<string> SpawnPackageModel( string packageName, Vector3 pos, Rotation rotation, GameObject source )
	{
		var package = await Package.Fetch( packageName, false );
		if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		if ( !source.IsValid ) return null; // source entity died or disconnected or something

		var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );
		var mins = package.GetMeta( "RenderMins", Vector3.Zero );
		var maxs = package.GetMeta( "RenderMaxs", Vector3.Zero );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		return model;
	}
}
