using System.Diagnostics;

public class Weld : BaseTool
{
	GameObject welded;
	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( welded == null )
		{
			welded = trace.GameObject;
			return true;
		}

		PropHelper propHelper = trace.GameObject.Components.Get<PropHelper>();

		if ( !propHelper.IsValid() )
			return false;

		propHelper.Weld( welded );

		welded = null;

		return true;
	}
	public override bool Secondary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		PropHelper propHelper = trace.GameObject.Components.Get<PropHelper>();

		if ( !propHelper.IsValid() )
			return false;

		propHelper.UnWeld();

		welded = null;

		return true;
	}
}
