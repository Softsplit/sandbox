using Sandbox.Services;

namespace Softsplit;

public class BoxShutter : ToolComponent
{
	private float scaleFactor = 1.0f;
	private float scaleSpeed = 0.01f;
	private Model model = Model.Load( "models/citizen_props/crate01.vmdl" );

	protected override void Start()
	{
		ToolName = "Box Shutter";
		ToolDes = "Scale Objects";
	}

	void PrimaryAction2()
	{
		var go = new GameObject();
		var prop = go.Components.Create<Prop>();
		prop.Model = model;
		go.Transform.Position = Player.AimRay.Position + Player.AimRay.Forward * 50;
		go.Components.Get<Rigidbody>().ApplyImpulse( Player.AimRay.Forward * 750000f );
		go.Components.Create<BreakOnPhysics>();
		go.NetworkSpawn();
		Stats.Increment( "box.shoot", 1 );
	}
	void SecondaryAction2()
	{
		model = Trace().GameObject.Components.Get<Prop>().Model;
	}

	protected override void OnInputUpdate()
	{

		if ( Input.Pressed( "Attack1" ) )
			PrimaryAction2();

		if ( Input.Down( "Attack2" ) )
			SecondaryAction2();

		if ( Input.Down( "Reload" ) )
			model = Model.Load( "models/citizen_props/crate01.vmdl" );

		base.OnInputDown();
	}
}
