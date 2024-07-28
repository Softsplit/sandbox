using Sandbox.Services;

namespace Softsplit;

public sealed class Camera : ToolComponent
{
	Vector3 pos;
	Rotation rotation;
	bool fixed_;
	bool right;
	float roll;
	float fov = 80;
	Model camera = Cloud.Model( "https://sbox.game/smlp/camera" );
	bool inp;
	protected override void Start()
	{
        ToolName = "Camera";
        ToolDes = "Camera Beta";        
	}
	protected override void PrimaryAction()
	{
		pos = Player.Camera.Transform.Position;
		rotation = Player.Camera.Transform.Rotation;
	}
	protected override void OnInputUpdate()
	{
		inp = true;
		base.OnInputUpdate();
		if ( Input.Pressed( "num0" ) )
			fixed_ ^= true;
		right = Input.Down( "attack2" );
	}
	protected override void OnPreRender()
	{
		if ( right && inp )
		{
			fov += 1.5f * Input.AnalogLook.pitch;
			roll += 1.5f * Input.AnalogLook.yaw;
		}
		if ( inp )
		{
			Player.Camera.FieldOfView = fov;
			Angles angl = Player.Camera.Transform.Rotation.Angles();
			angl.roll = roll;
			Player.Camera.Transform.Rotation = angl.ToRotation();
		}
		if ( !fixed_ )
			return;
		Gizmo.Draw.Model( camera, new Transform( pos, rotation ) );
		Player.Camera.Transform.Position = pos;
		Player.Camera.Transform.Rotation = rotation;
		inp = false;
	}
}
