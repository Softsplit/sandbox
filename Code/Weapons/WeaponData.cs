/// <summary>
/// A resource definition for a piece of equipment. This could be a weapon, or a deployable, or a gadget, or a grenade.. Anything really.
/// </summary>
[GameResource( "sandbox/Equipment Item", "equip", "", IconBgColor = "#5877E0", Icon = "track_changes" )]
public partial class EquipmentResource : GameResource
{
	public static HashSet<EquipmentResource> All { get; set; } = new();

	[Category( "Base" )]
	public string Name { get; set; } = "My Equipment";

	[Category( "Base" )]
	public string Description { get; set; } = "";

	/// <summary>
	/// If true, owner will drop this equipment if they disconnect.
	/// </summary>
	[Category( "Base" )]
	public bool DropOnDisconnect { get; set; } = false;

	/// <summary>
	/// The equipment's icon
	/// </summary>
	[Group( "Base" ), ImageAssetPath] public string Icon { get; set; }

	/// <summary>
	/// The prefab to create and attach to the player when spawning it in.
	/// </summary>
	[Category( "Prefabs" )]
	public GameObject MainPrefab { get; set; }

	/// <summary>
	/// The prefab to create when making a viewmodel for this equipment.
	/// </summary>
	[Category( "Prefabs" )]
	public GameObject ViewModelPrefab { get; set; }

	/// <summary>
	/// The equipment's model
	/// </summary>
	[Category( "Information" )]
	public Model WorldModel { get; set; }

	[Category( "Dropping" )]
	public Vector3 DroppedSize { get; set; } = new( 8, 2, 8 );

	[Category( "Dropping" )]
	public Vector3 DroppedCenter { get; set; } = new( 0, 0, 0 );

	protected override void PostLoad()
	{
		if ( All.Contains( this ) )
		{
			Log.Warning( "Tried to add two of the same equipment (?)" );
			return;
		}

		All.Add( this );
	}
}
