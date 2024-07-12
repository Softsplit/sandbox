public class Scale : Tool
{
	private float scaleFactor = 1.0f;
	private float scaleSpeed = 0.01f;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( !IsUsing() )
		{
			return;
		}

		if ( Input.Down( "attack1" ) )
		{
			scaleFactor = 1.0f + scaleSpeed;
			ScaleObject( scaleFactor );
		}
		else if ( Input.Down( "attack2" ) )
		{
			scaleFactor = 1.0f - scaleSpeed;
			ScaleObject( scaleFactor );
		}
	}

	private void ScaleObject( float scaleFactor )
	{
		var trace = DoTrace();
		if ( trace.Hit )
		{
			var prop = trace.GameObject.Components.Get<Prop>();
			if ( prop != null )
			{
				trace.GameObject.Transform.Scale *= scaleFactor;
				// trace.Body.Mass *= scaleFactor;
			}
		}
	}
}
