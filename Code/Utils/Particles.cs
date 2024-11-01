
public static class Particles
{
	public static LegacyParticleSystem Create( string specParticle, GameObject parent, Rotation rot = default, bool client = true, float decay = 5f )
	{
		var gameObject = Game.ActiveScene.CreateObject();
		if ( !client )
			gameObject.NetworkSpawn();
		gameObject.Parent = parent;
		gameObject.WorldRotation = rot;

		var particle = gameObject.Components.GetOrCreate<LegacyParticleSystem>();
		if ( particle.IsValid )
		{
			particle.Particles = ParticleSystem.Load( specParticle );
		}
		
		particle.Destroy();
		// Clear off in a suitable amount of time.
		gameObject.DestroyAsync( decay );

		return particle;
	}
	public static LegacyParticleSystem Create( string specParticle, Vector3 pos, Rotation rot = default, bool client = true, float decay = 5f )
	{
		var gameObject = Game.ActiveScene.CreateObject();
		if ( !client )
			gameObject.NetworkSpawn();
		gameObject.WorldPosition = pos;
		gameObject.WorldRotation = rot;

		var particle = gameObject.Components.Create<LegacyParticleSystem>();
		particle.Particles = ParticleSystem.Load( specParticle );
		gameObject.Transform.ClearInterpolation();

		// Clear off in a suitable amount of time.
		gameObject.DestroyAsync( decay );

		return particle;
	}
}
