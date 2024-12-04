public static class Particles
{
	public static LegacyParticleSystem CreateParticleSystem( string path, Transform transform, float time = 1, GameObject parent = null )
	{
		SpawnParticleSystem( Connection.Local.Id, path, transform.Position, transform.Rotation, time, parent );

		return MakeParticleSystem( path, transform, time, parent );
	}

	[Rpc.Broadcast(NetFlags.Unreliable)]
	public static void SpawnParticleSystem( Guid connection, string path, Vector3 position, Rotation rotation, float time = 1, GameObject parent = null )
	{
		if ( Connection.Local.Id == connection )
			return;

		MakeParticleSystem( path, new Transform( position, rotation ), time, parent );
	}

	public static LegacyParticleSystem MakeParticleSystem( string path, Transform transform, float time = 1, GameObject parent = null )
	{
		var particleSystem = ParticleSystem.Load( path );

		if ( !particleSystem.IsValid() )
			return null;

		var go = new GameObject
		{
			Name = particleSystem.Name,
			Parent = parent,
			WorldTransform = transform,
			NetworkMode = NetworkMode.Never
		};

		var legacyParticleSystem = go.AddComponent<LegacyParticleSystem>();
		legacyParticleSystem.Particles = particleSystem;
		legacyParticleSystem.ControlPoints = new()
		{
			new ParticleControlPoint { GameObjectValue = go, Value = ParticleControlPoint.ControlPointValueInput.GameObject }
		};

		if ( time > 0 )
			go.DestroyAsync( time );

		return legacyParticleSystem;
	}
}
