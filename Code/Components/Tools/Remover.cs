public class Remover : BaseTool
{
	public override bool Primary( SceneTraceResult Trace )
	{
		if ( Trace.GameObject.Components.Get<PropHelper>() == null )
			return false;

		Remove( Trace.GameObject );

		return true;
	}

	[Broadcast]
	void Remove( GameObject g )
	{
		g.Destroy();
	}
}
