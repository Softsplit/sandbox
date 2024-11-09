public abstract class BaseTool : Component
{
	public ToolGun Parent { get; set; }
	public Player Owner { get; set; }

	public virtual bool Primary( SceneTraceResult trace )
	{
		return false;
	}

	public virtual bool Secondary( SceneTraceResult trace )
	{
		return false;
	}

	public virtual bool Reload( SceneTraceResult trace )
	{
		return false;
	}

	public virtual void Disabled()
	{

	}
}
