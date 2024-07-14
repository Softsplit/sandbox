namespace Softsplit;

public partial class PlayerLoadout : Component
{
	[Property] public PlayerState PlayerState { get; set; }

	[Property] public List<EquipmentResource> Equipment { get; set; }

	/// <summary>
	/// Clears the player's loadout equipment.
	/// </summary>
	public void SetFrom( PlayerPawn playerPawn )
	{
		Equipment.Clear();
		Equipment.AddRange( playerPawn.Inventory.Equipment.Select( x => x.Resource ) );
	}
}
