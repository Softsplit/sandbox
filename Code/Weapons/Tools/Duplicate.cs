namespace Softsplit;

public sealed class Duplicate : ToolComponent
{
	GameObject storedObject;
	int pressed = 0;

	protected override void Start()
	{
		ToolName = "Duplicate";
		ToolDes = "Duplicates an object";
	}

	

	[Broadcast]
	protected override void PrimaryAction()
	{
		 if ( !Networking.IsHost )
            return;
		
		var hit = Scene.Trace.Ray( WeaponRay.Position, WeaponRay.Position + WeaponRay.Forward * 500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "movement", "player_clip" )
			.Run();
		pressed++;

		if (hit.Hit) 
		{
			
			if (pressed == 1) 
			{
				if (hit.GameObject.Tags.Has( "map" )) 
				{
					pressed = 0;
				}
				else
				{
					Recoil(hit.EndPosition);
					storedObject = hit.GameObject.Clone();
				}		
			}
			else if (pressed == 2)
			{
				storedObject.Transform.Position = hit.HitPosition + storedObject.Transform.Scale.z;
				storedObject.Transform.Rotation = Rotation.LookAt(Player.Body.GameObject.Transform.Position - storedObject.Transform.Position);
				Recoil(hit.EndPosition);
				storedObject.NetworkSpawn();
				storedObject = null;
				pressed = 0;
			}
		}
	}

	protected override void SecondaryAction()
	{

	}
}
