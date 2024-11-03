public class Weld : BaseTool
{
	GameObject welded;
	public override bool Primary( SceneTraceResult Trace )
	{
		if ( welded == null )
		{
			welded = Trace.GameObject;
			return true;
		}

		if ( welded == Trace.GameObject )
			return false;

		PropHelper propHelper = Trace.GameObject.Components.Get<PropHelper>();

		bool noProp = !propHelper.IsValid();

		if(noProp)
			propHelper = welded.GetComponent<PropHelper>();

		if ( !propHelper.IsValid() )
			return false;

		propHelper.Weld( noProp ? Trace.GameObject : welded );

		welded = null;

		return true;
	}
	public override bool Secondary( SceneTraceResult Trace )
	{
		PropHelper propHelper = Trace.GameObject.Components.Get<PropHelper>();

		if ( !propHelper.IsValid() )
			return false;

		propHelper.UnWeld();

		welded = null;

		return true;
	}
}
