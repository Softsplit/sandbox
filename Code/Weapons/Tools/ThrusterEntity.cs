using Softsplit;

public sealed class ThrusterEntity : Component, IUse
{
	SceneParticles effects;
	[Sync] bool Work { get; set; } = true;

	public Guid localPawn {get;set;}
	public float Force {get;set;}
	public string Forward {get;set;}
	public string Backward {get;set;}
	public bool Toggle {get;set;}
	[Sync] public bool on {get;set;}
	[Sync] public bool direction {get;set;}



	void Inputs()
	{
		
		if(Toggle)
		{
			if(Input.Pressed(Forward))
			{
				on = !on;
				direction = true;
			}
			if(Input.Pressed(Backward))
			{
				on = !on;
				direction = false;
			}
		}
		else
		{
			if(Input.Down(Forward))
			{
				on = true;
				direction = true;
			}
			else if(Input.Down(Backward))
			{
				on = true;
				direction = false;
			}
			else
			{
				on = false;
			}
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		effects = new SceneParticles( Scene.SceneWorld, "particles/physgun_end_nohit.vpcf" );
	}

	protected override void OnUpdate()
	{
		if(PlayerState.Local.Pawn.Id == localPawn)
		{
			if(Network.OwnerId != localPawn) foreach (GameObject g in PhysGunComponent.GetAllConnectedWelds(GameObject)) g.Network.TakeOwnership();
		}
		else return;

		Inputs();
		if ( !Components.GetInChildrenOrSelf<Rigidbody>().MotionEnabled || !Work )
			return;
		
		if(on)
		{
			if(!effects.IsValid()) effects = new SceneParticles( Scene.SceneWorld, "particles/physgun_end_nohit.vpcf" );
			effects.SetControlPoint( 0, Transform.Position + (direction ? Transform.Rotation.Up : Transform.Rotation.Down) * 20f );
			effects.Simulate( Time.Delta );
			Vector3 velocity = Components.GetInChildrenOrSelf<Rigidbody>().Velocity;
			velocity = Vector3.Lerp( velocity, Transform.Rotation.Down * (direction ? Force : -Force), 0.4f );
			Components.GetInChildrenOrSelf<Rigidbody>().Velocity = velocity;
		}
		else
		{
			if(effects.IsValid())
			{
				effects.Delete();
				effects = null;
			}
		}
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
