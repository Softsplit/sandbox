
public class Weld : BaseTool
{
    GameObject welded;
    Vector3 point1;
	public override void Primary( SceneTraceResult Trace )
	{
		if(welded == null)
        {
            welded = Trace.GameObject;
            point1 = Trace.GameObject.Transform.World.PointToLocal(Trace.EndPosition);
            return;
        }

        PropHelper propHelper = Trace.GameObject.Components.Get<PropHelper>();

        if(!propHelper.IsValid())
            return;

        if(!Trace.GameObject.Components.Get<Rigidbody>().IsValid())
            return;
        
        propHelper.Weld(welded, propHelper.Transform.World.PointToLocal(Trace.EndPosition), point1);

        welded = null;
	}
	public override void Secondary( SceneTraceResult Trace )
	{
		PropHelper propHelper = Trace.GameObject.Components.Get<PropHelper>();

        if(!propHelper.IsValid())
            return;

        if(!Trace.GameObject.Components.Get<Rigidbody>().IsValid())
            return;
        
        propHelper.UnWeld();
	}
}