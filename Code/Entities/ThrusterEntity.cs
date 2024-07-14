using Softsplit;

public sealed class ThrusterEntity : Component, Softsplit.IUse
{
	SceneParticles effects;
	[Sync] bool Work { get; set; } = true;

	protected override void OnAwake()
	{
		base.OnAwake();
		effects = new SceneParticles( Scene.SceneWorld, "particles/physgun_end_nohit.vpcf" );
	}

	protected override void OnUpdate()
	{
		if ( !Components.GetInChildrenOrSelf<Rigidbody>().MotionEnabled || !Work )
			return;
		effects.SetControlPoint( 0, Transform.Position + Transform.Rotation.Up * 20f );
		// effects.SetControlPoint( 0, Transform.Rotation );
		effects.Simulate( Time.Delta );
		Vector3 velocity = Components.GetInChildrenOrSelf<Rigidbody>().Velocity;
		velocity = Vector3.Lerp( velocity, Transform.Rotation.Down * 600f, 0.4f );
		Components.GetInChildrenOrSelf<Rigidbody>().Velocity = velocity;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		effects.Delete();
	}

	public bool IsUsable( GameObject user )
	{
		return true;
	}

	public bool CanUse( PlayerPawn player )
	{
		return true;
	}

	public void OnUse( PlayerPawn player )
	{
		Work = !Work;
		if ( !Work )
		{
			effects.Delete();
		}
		else
		{
			effects = new SceneParticles( Scene.SceneWorld, "particles/physgun_end_nohit.vpcf" );
		}
	}
}
