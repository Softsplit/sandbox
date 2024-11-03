public class Weld : BaseTool
{
	GameObject welded;
	public override void Primary( SceneTraceResult Trace )
	{
		if ( welded == null )
		{
			welded = Trace.GameObject;
			return;
		}

		PropHelper propHelper = Trace.GameObject.Components.Get<PropHelper>();

		if ( !propHelper.IsValid() )
			return;

		propHelper.Weld( welded );

		welded = null;
	}
	public override void Secondary( SceneTraceResult Trace )
	{
		PropHelper propHelper = Trace.GameObject.Components.Get<PropHelper>();

		if ( !propHelper.IsValid() )
			return;

		propHelper.UnWeld();

		welded = null;
	}
}
