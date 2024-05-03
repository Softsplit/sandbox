public sealed class Unstuck : Component
{
	[Sync] public bool IsActive { get; set; } // replicate

	internal int StuckTries = 0;

	public bool TestAndFix()
	{
		var result = Components.Get<PlayerController>().TraceBBox( Transform.Position, Transform.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		if ( result.StartedSolid )
		{
			if ( PlayerController.Debug )
			{
				/*
				DebugOverlay.Text( $"[stuck in {result.Entity}]", Transform.Position, Color.Red );
				DebugOverlay.Box( result.Entity, Color.Red );
				*/
			}
		}

		int AttemptsPerTick = 20;

		for ( int i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Transform.Position + Vector3.Random.Normal * (StuckTries / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
			{
				pos = Transform.Position + Vector3.Up * 5;
			}

			result = Components.Get<PlayerController>().TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				if ( PlayerController.Debug )
				{
					/*
					DebugOverlay.Text( $"unstuck after {StuckTries} tries ({StuckTries * AttemptsPerTick} tests)", Controller.Position, Color.Green, 5.0f );
					DebugOverlay.Line( pos, Controller.Position, Color.Green, 5.0f, false );
					*/
				}

				Transform.Position = pos;
				return false;
			}
			else
			{
				if ( PlayerController.Debug )
				{
					/*
					DebugOverlay.Line( pos, Controller.Position, Color.Yellow, 0.5f, false );
					*/
				}
			}
		}

		StuckTries++;

		return true;
	}
}
