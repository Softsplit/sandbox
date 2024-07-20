namespace Softsplit;

public class Scale : ToolComponent
{
	private float scaleFactor = 1.0f;
	private float scaleSpeed = 0.01f;

	protected override void Start()
	{
		ToolName = "Scaler";
		ToolDes = "Scale Objects";
	}

	void PrimaryAction2()
	{
		scaleFactor = 1.0f + scaleSpeed;
		ScaleObject( scaleFactor );
	}
	void SecondaryAction2()
	{
		scaleFactor = 1.0f - scaleSpeed;
		ScaleObject( scaleFactor );
	}

	protected override void OnInputUpdate()
	{

		if ( Input.Down( "Attack1" ) )
			PrimaryAction2();

		if ( Input.Down( "Attack2" ) )
			SecondaryAction2();

		base.OnInputDown();
	}

	private void ScaleObject( float scaleFactor )
	{
		var trace = Trace();
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
