public abstract class BaseTool : Component
{
	public virtual bool Primary( SceneTraceResult Trace )
	{
		return false;
	}

	public virtual bool Secondary( SceneTraceResult Trace )
	{
		return false;
	}
}
