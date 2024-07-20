namespace Softsplit;

public sealed class RemoverAll : ToolComponent
{
	protected override void Start()
	{
        ToolName = "Remove All";
        ToolDes = "Remove all objects.";        
	}
	protected override void PrimaryAction()
	{
        if (!Networking.IsHost)
            return;
        var sound = Sound.Play("sounds/balloon_pop_cute.sound", Player.Body.Transform.Position);
        sound.Volume = 0.25f;
        foreach (var obj in Scene.GetAllObjects(true))
        {
            if ( obj.Tags.Has( "prop" ) ) 
            {
                RemoveObject(obj);
            }
        }
	}

    [Broadcast]
    public static void RemoveObject(GameObject gameObject)
    {
        gameObject.Destroy();
    }
}
