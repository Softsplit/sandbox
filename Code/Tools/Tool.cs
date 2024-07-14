namespace Tools;
public class Tool : Softsplit.Equipment
{
	/*
	[Sync]
	public Tool Parent { get; set; }
	*/
	protected virtual float MaxTraceDistance => 10000.0f;

	protected override void OnStart()
	{
		Resource = new EquipmentResource { };
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
	public bool IsUsing()
	{
		return Owner != null && Owner.ActiveChild == this;
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
