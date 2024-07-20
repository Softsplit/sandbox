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
        var hit = Trace();
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
