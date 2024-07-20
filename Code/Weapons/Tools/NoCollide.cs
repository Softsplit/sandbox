namespace Softsplit;

public sealed class NoCollide : ToolComponent
{
	protected override void Start()
	{
        ToolName = "No Collider";
        ToolDes = "Remove collision between objects.";        
	}
    protected override void PrimaryAction()
    {
        var hit = Trace();
        foreach(string s in hit.Tags)
        {
            Log.Info(s);
        }
        if(hit.Hit)
        {
            if(hit.GameObject.Tags.Contains("propcollide"))
            {
                hit.GameObject.Tags.Remove("propcollide");
                hit.GameObject.Tags.Add("nopropcollide");
                Recoil(hit.EndPosition);
            }
        }
    }
	protected override void SecondaryAction()
	{
        var hit = Trace();
        if(hit.Hit)
        {
            if(!hit.GameObject.Tags.Contains("nocollide"))
            {
                hit.GameObject.Tags.Remove("propcollide");
                hit.GameObject.Tags.Remove("nopropcollide");
                hit.GameObject.Tags.Remove("solid");
                hit.GameObject.Tags.Add("nocollide");
                Recoil(hit.EndPosition);
            }
        }
	}
}
