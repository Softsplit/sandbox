using Sandbox.Events;
using Sandbox.Physics;

namespace Softsplit;

public sealed class Remover : ToolComponent
{
	protected override void Start()
	{
        ToolName = "Remover";
        ToolDes = "Remove Objects.";        
	}
	protected override void PrimaryAction()
	{
        var hit = Scene.Trace.Ray( WeaponRay.Position, WeaponRay.Position+WeaponRay.Forward*500 )
			.UseHitboxes()
			.IgnoreGameObjectHierarchy( GameObject.Root )
			.WithoutTags( "trigger", "invis", "ragdoll", "movement", "player_clip" )
			.Run();
        if(hit.Hit && !hit.Tags.Contains("map"))
        {
            Recoil(hit.EndPosition);
            
            RemoveObject(hit.GameObject);

            Weld.RemoveWeld(hit.GameObject);
        }
	}

    [Broadcast]
    public static void RemoveObject(GameObject gameObject)
    {
        Sound.Play( "sounds/balloon_pop_cute.sound", gameObject.Transform.Position );
        gameObject.Destroy();
    }
}
