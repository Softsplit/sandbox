using System;

[Spawnable]
[Library( "ent_bouncyball", Title = "Bouncy Ball" )]
public partial class BouncyBallEntity : Prop, Component.ICollisionListener, IUse
{
	[Property] public float MaxSpeed { get; set; } = 1000.0f;
	[Property] public float SpeedMul { get; set; } = 1.2f;

	protected override void OnStart()
	{
		Model = Model.Load( "models/ball/ball.vmdl" );
		Components.Get<ModelRenderer>().Tint = Color.Random;

		Transform.Scale = Game.Random.Float( 0.5f, 2.0f );
	}

	public void OnCollisionStart( Collision other )
	{
		var speed = other.Contact.Speed.Length;
		var direction = Vector3.Reflect( other.Contact.Speed.Normal, other.Contact.Normal.Normal ).Normal;
		Components.Get<Rigidbody>().Velocity = direction * MathF.Min( speed * SpeedMul, MaxSpeed );
	}

	public bool IsUsable( GameObject user )
	{
		return true;
	}

	public bool OnUse( GameObject user )
	{
		if ( user.Components.TryGet<Player>( out var player ) )
		{
			player.Health += 10;

			GameObject.Destroy();
		}

		return false;
	}
}
