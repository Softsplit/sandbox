public sealed class Duck : Component
{
	[Sync] public bool IsActive { get; set; } // replicate

	public void PreTick()
	{
		Tags.Set( "ducked", IsActive );

		bool wants = Input.Down( "duck" );

		if ( wants != IsActive )
		{
			if ( wants ) TryDuck();
			else TryUnDuck();
		}

		if ( IsActive )
		{
			Components.Get<Player>().EyeLocalPosition *= 0.5f;
		}
	}

	void TryDuck()
	{
		IsActive = true;
	}

	void TryUnDuck()
	{
		var pm = Components.Get<PlayerController>().TraceBBox( Transform.Position, Transform.Position, originalMins, originalMaxs );
		if ( pm.StartedSolid ) return;

		IsActive = false;
	}

	// Uck, saving off the bbox kind of sucks
	// and we should probably be changing the bbox size in PreTick
	Vector3 originalMins;
	Vector3 originalMaxs;

	public void UpdateBBox( ref Vector3 mins, ref Vector3 maxs )
	{
		originalMins = mins;
		originalMaxs = maxs;

		if ( IsActive )
			maxs = maxs.WithZ( 36 ) * Transform.Scale;
	}

	//
	// Coudl we do this in a generic callback too?
	//
	public float GetWishSpeed()
	{
		if ( !IsActive ) return -1;
		return 60.0f;
	}
}
