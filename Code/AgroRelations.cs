using Sandbox;

public sealed class AgroRelations : Component
{
	[Property] public string Faction {get; set;}
	[Property] public List<string> Enemies {get; set;}
	[Property] public Vector3 attackPoint {get; set;}
}
