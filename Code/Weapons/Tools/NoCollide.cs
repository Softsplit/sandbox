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
           NoPropCollideTags(hit.GameObject,hit.EndPosition);
        }
    }
	protected override void SecondaryAction()
	{
        var hit = Trace();
        if(hit.Hit)
        {
            NoCollideTags(hit.GameObject,hit.EndPosition);
        }
	}

    [Broadcast]
    public void NoPropCollideTags(GameObject gameObject, Vector3 endPos)
    {
        if(gameObject.Tags.Contains("propcollide"))
        {
            gameObject.Tags.Remove("propcollide");
            gameObject.Tags.Add("nopropcollide");
            Recoil(endPos);
        }
    }

    [Broadcast]
    public void NoCollideTags(GameObject gameObject, Vector3 endPos)
    {
        if(gameObject.Tags.Contains("nocollide"))
        {
            gameObject.Tags.Remove("propcollide");
            gameObject.Tags.Remove("nopropcollide");
            gameObject.Tags.Remove("solid");
            gameObject.Tags.Add("nocollide");
            Recoil(endPos);
        }
    }
}
