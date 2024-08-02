namespace Softsplit.Ents;
public class Flyer : Component, IUse
{
	private float scaleFactor = 1.0f;
	private float scaleSpeed = 0.01f;
	private PlayerPawn player;
	private Vector3 offset;
	private Guid player_id { get; set; }
	[Property] private Rigidbody rb;

	void Start()
	{
		Network.DropOwnership();
		rb.PhysicsBody.BodyType = PhysicsBodyType.Static;
	}
	void End()
	{
		// rb.PhysicsBody.BodyType = PhysicsBodyType.Dynamic;
	}
	protected override void OnFixedUpdate()
	{
		if ( rb != null && player != null )
		{
			GameObject.Transform.Position.LerpTo( player.Transform.Position + offset, 0.7f );
			GameObject.Transform.Position += player.CharacterController.Velocity * Time.Delta;
			if ( Input.Down( "Jump" ) )
			{
				GameObject.Transform.Position += Vector3.Up * 100f * Time.Delta;
			}
			if ( Input.Down( "Duck" ) )
			{
				GameObject.Transform.Position += Vector3.Down * 150f * Time.Delta;
			}
		}
	}

	public bool CanUse( PlayerPawn player )
	{
		return true;
	}

	public void OnUse( PlayerPawn player )
	{
		// TODO: make this code readable
		if (this.player != null)
		{
			End();
			this.player = null;
			this.player_id = Guid.Empty;
			return;
		}
		Start();
		offset = player.Transform.Position - Transform.Position;
		this.player_id = player.Network.OwnerId;
		this.player = player;
	}
}
