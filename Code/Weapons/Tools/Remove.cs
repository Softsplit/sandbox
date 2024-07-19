namespace Softsplit;
public class Remove : ToolComponent
{

	protected override void Start()
	{
		ToolName = "Remove";
		ToolDes = "Delete Objects";
	}
	protected override void PrimaryAction()
	{
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
