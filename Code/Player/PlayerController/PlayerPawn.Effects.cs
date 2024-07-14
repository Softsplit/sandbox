namespace Softsplit;

public partial class PlayerPawn
{
	/// <summary>
	/// What effect should we spawn when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject HeadshotEffect { get; set; }

	/// <summary>
	/// What effect should we spawn when we hit a player?
	/// </summary>
	[Property, Group( "Effects" )] public GameObject BloodEffect { get; set; }

	/// <summary>
	/// What sound should we play when a player gets headshot?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent HeadshotSound { get; set; }

	/// <summary>
	/// What sound should we play when we hit a player?
	/// </summary>
	[Property, Group( "Effects" )] public SoundEvent BloodImpactSound { get; set; }
}
