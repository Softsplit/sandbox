
public class Remover : BaseTool
{
	public override void Primary( SceneTraceResult Trace )
	{
        if(Trace.GameObject.Components.Get<PropHelper>() == null)
            return;
        
		Remove(Trace.GameObject);
	}

    [Broadcast]
    void Remove(GameObject g)
    {
        g.Destroy();
    }
}