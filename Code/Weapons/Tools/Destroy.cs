namespace Softsplit;

public sealed class Destroy : ToolComponent
{

	protected override void Start()
	{
		ToolName = "Destroy";
		ToolDes = "Destroys the object, which is pointed at.";
	}

	protected override void PrimaryAction()
	{
		var hit = Scene.Trace.Ray( WeaponRay.Position, WeaponRay.Position+WeaponRay.Forward*500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll", "movement", "player_clip" )
			.Run();
		if (hit.Hit)
		{
			if (!hit.GameObject.Tags.Has("map"))
			{
				Recoil(hit.EndPosition);
				hit.GameObject.Destroy();
			}
				
		}
	}

	protected override void SecondaryAction()
	{

	}
}
