namespace Softsplit;

/// <summary>
/// A component that handles what a player wears.
/// </summary>
public partial class PlayerOutfitter : Component, Component.INetworkSpawn
{
	/// <summary>
	/// The player's body component.
	/// </summary>
	[RequireComponent] public PlayerBody Body { get; set; }

	/// <summary>
	/// We store the player's avatar over the network so everyone knows what everyone looks like.
	/// </summary>
	[Sync] public string Avatar { get; set; }

	/// <summary>
	/// Grab the player's avatar data.
	/// </summary>
	/// <param name="owner"></param>
	public void OnNetworkSpawn( Connection owner )
	{
		Avatar = owner.GetUserData( "avatar" );

		var container = new ClothingContainer();
		container.Deserialize( Avatar );
		container.Apply( Body.Renderer );
	}
}
