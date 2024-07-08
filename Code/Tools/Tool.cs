public class Tool : Component
{
	/*
	[Sync]
	public Tool Parent { get; set; }
	*/

	// [Sync]
	public Player Owner { get; set; }

	protected virtual float MaxTraceDistance => 10000.0f;

	protected override void OnStart()
	{
		if ( Connection.Local.IsHost )
		{
			// CreatePreviews();
		}
	}

	public virtual void Deactivate()
	{
		// DeletePreviews();
	}

	public virtual void OnDraw()
	{
		// UpdatePreviews();
	}

	public virtual void CreateHitEffects( Vector3 pos )
	{
		// Parent?.CreateHitEffects( pos );
	}

	public SceneTraceResult DoTrace()
	{
		return Scene.Trace.Ray( Owner.AimRay, MaxTraceDistance )
			//.WithAnyTags( "solid", "nocollide" )
			.WithoutTags( "player" )
			.IgnoreGameObject( Owner.GameObject )
			.Run();
	}
}
