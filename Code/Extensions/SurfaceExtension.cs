namespace Sandbox;

/// <summary>
/// Extensions for Surfaces
/// </summary>
public static partial class SandboxBaseExtensions
{
	/// <summary>
	/// Create a particle effect and play an impact sound for this surface being hit by a bullet
	/// </summary>
	public static SceneParticles DoBulletImpact( this Surface self, SceneTraceResult tr )
	{
		//
		// No effects on resimulate
		//
		/*
		if ( !Prediction.FirstTime )
			return null;
		*/

		//
		// Drop a decal
		//
		var decalPath = Game.Random.FromList( self.ImpactEffects.BulletDecal );

		var surf = self.GetBaseSurface();
		while ( string.IsNullOrWhiteSpace( decalPath ) && surf != null )
		{
			decalPath = Game.Random.FromList( surf.ImpactEffects.BulletDecal );
			surf = surf.GetBaseSurface();
		}

		if ( !string.IsNullOrWhiteSpace( decalPath ) )
		{
			if ( ResourceLibrary.TryGet<DecalDefinition>( decalPath, out var decal ) )
			{
				// Decal.Place( decal, tr );
			}
		}

		//
		// Make an impact sound
		//
		var sound = self.Sounds.Bullet;

		surf = self.GetBaseSurface();
		while ( string.IsNullOrWhiteSpace( sound ) && surf != null )
		{
			sound = surf.Sounds.Bullet;
			surf = surf.GetBaseSurface();
		}

		if ( !string.IsNullOrWhiteSpace( sound ) )
		{
			Sound.Play( sound, tr.EndPosition );
		}

		//
		// Get us a particle effect
		//

		string particleName = Game.Random.FromList( self.ImpactEffects.Bullet );
		if ( string.IsNullOrWhiteSpace( particleName ) ) particleName = Game.Random.FromList( self.ImpactEffects.Regular );

		surf = self.GetBaseSurface();
		while ( string.IsNullOrWhiteSpace( particleName ) && surf != null )
		{
			particleName = Game.Random.FromList( surf.ImpactEffects.Bullet );
			if ( string.IsNullOrWhiteSpace( particleName ) ) particleName = Game.Random.FromList( surf.ImpactEffects.Regular );

			surf = surf.GetBaseSurface();
		}

		if ( !string.IsNullOrWhiteSpace( particleName ) )
		{
			/*
			var ps = Particles.Create( particleName, tr.EndPosition );
			ps.SetForward( 0, tr.Normal );

			return ps;
			*/
		}

		return default;
	}
}
