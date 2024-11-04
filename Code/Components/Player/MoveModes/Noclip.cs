namespace Sandbox.Movement;

[Icon( "flight" ), Group( "Movement" ), Title( "MoveMode - Noclip" )]
public class MoveModeNoclip : MoveMode
{
	[Property]
	public int Priority { get; set; } = 100;

	public override int Score( PlayerController controller )
	{
		if ( Tags.Has( "noclip" ) ) return Priority;
		return -100;
	}

	public override void UpdateRigidBody( Rigidbody body )
	{
		body.Gravity = false;
		body.Velocity = Controller.WishVelocity;
	}

	public override Vector3 UpdateMove( Rotation eyes, Vector3 input )
	{
		var wishVelocity = eyes * input * Controller.RunSpeed;
		if ( Input.Down( "run" ) ) wishVelocity *= 5.0f;
		if ( Input.Down( "duck" ) ) wishVelocity *= 0.2f;

		if ( Input.Down( "jump" ) )
		{
			wishVelocity += Vector3.Up * Controller.JumpSpeed;
		}
		
		return wishVelocity;
	}
}