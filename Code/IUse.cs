using Sandbox;

/// <summary>
/// If an entity implements this it'll be interactable, typically by looking at it and pressing the USE button,
/// but the implementation will depend on the currently active game.
/// </summary>
public interface IUse
{
	/// <summary>
	/// Called when the (probably) player has used this entity.
	/// </summary>
	/// <param name="user">The entity that interacted with us. It is <b>not</b> guaranteed to be a player.</param>
	/// <returns>Return true if the player should continually use this entity. Return false when the player should stop using it.
	/// <para>For example - a health charger will return true while the player is taking health.
	/// We're passing the player in as an entity so at some point
	/// if we want NPCs using shit, we can do that without the assumption.</para></returns>
	bool OnUse( GameObject user );

	/// <summary>
	/// Dictates whether this entity is usable by given user.
	/// </summary>
	/// <param name="user">The entity that wants to interact with us. It is <b>not</b> guaranteed to be a player.</param>
	/// <returns>Return true if the given entity can use/interact with this entity.</returns>
	bool IsUsable( GameObject user );
}
