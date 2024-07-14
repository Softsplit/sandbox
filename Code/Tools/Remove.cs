namespace Tools;
public class Remove : Tool
{
	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( !IsUsing() )
		{
			return;
		}
		if ( Input.Pressed( "attack1" ) )
		{
			var trace = DoTrace();
			if ( trace.Hit )
			{
				var prop = trace.GameObject.Components.Get<Prop>();
				if ( prop != null )
				{
					trace.GameObject.Destroy();
				}
			}
		}
	}
}
