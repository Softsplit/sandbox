namespace Sandbox.Movement;

[Icon( "flight" ), Group( "Movement" ), Title( "MoveMode - Noclip" )]
public partial class MoveModeNoclip : MoveMode
{
	[Property] public int Priority { get; set; } = 1;

	public override int Score( PlayerController controller )
	{
		if ( Controller.IsNoclipping ) return Priority;
		return -100;
	}

	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Enabled = !Controller.IsNoclipping;
		Controller.ColliderObject.Enabled = !Controller.IsNoclipping;
	}

	public override void AddVelocity()
	{
		var fwd = Input.AnalogMove.x.Clamp( -1f, 1f );
		var left = Input.AnalogMove.y.Clamp( -1f, 1f );
		var rotation = Controller.EyeAngles.ToRotation();

		var vel = (rotation.Forward * fwd) + (rotation.Left * left);

		if ( Input.Down( "jump" ) )
		{
			vel += Vector3.Up * 1;
		}

		vel = vel.Normal * 20000;

		if ( Input.Down( "run" ) )
			vel *= 5.0f;

		if ( Input.Down( "duck" ) )
			vel *= 0.2f;

		Controller.Velocity += vel * Time.Delta;

		if ( Controller.Velocity.LengthSquared > 0.01f )
		{
			WorldPosition += Controller.Velocity * Time.Delta;
		}

		Controller.Velocity = Controller.Velocity.Approach( 0, Controller.Velocity.Length * Time.Delta * 5.0f );
		Controller.EyeAngles = rotation;
		Controller.WishVelocity = Controller.Velocity;
		Controller.GroundObject = null;
		Controller.GroundVelocity = Vector3.Zero;
	}
}
