public enum LifeState
{
	/// <summary>
	/// Alive as normal
	/// </summary>
	Alive,
	/// <summary>
	/// Playing a death animation
	/// </summary>
	Dying,
	/// <summary>
	/// Dead, lying still
	/// </summary>
	Dead,
	/// <summary>
	/// Can respawn, usually waiting for some client action to respawn
	/// </summary>
	Respawnable,
	/// <summary>
	/// Is in the process of respawning
	/// </summary>
	Respawning
}
