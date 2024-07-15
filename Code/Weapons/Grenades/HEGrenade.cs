namespace Softsplit;

[Title( "HE Grenade" )]
public partial class HEGrenade : BaseGrenade
{
	[Property] public float DamageRadius { get; set; } = 512f;
	[Property] public float MaxDamage { get; set; } = 100f;
	[Property] public Curve DamageFalloff { get; set; } = new Curve( new Curve.Frame( 1.0f, 1.0f ), new Curve.Frame( 0.0f, 0.0f ) );

	protected override void Explode()
	{
		if ( Networking.IsHost )
			Explosion.AtPoint( Transform.Position, DamageRadius, MaxDamage, Player, this, DamageFalloff );

		base.Explode();
	}
}
