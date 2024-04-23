using Sandbox;
using System;

[Spawnable]
[Library( "ent_bouncyball", Title = "Bouncy Ball" )]
public partial class BouncyBallEntity : Component, Component.ICollisionListener, IUse
{
	[Property] public float MaxSpeed { get; set; } = 1000.0f;
	[Property] public float SpeedMul { get; set; } = 1.2f;

	protected override void OnStart()
	{
		Components.Create<ModelRenderer>().Model = Model.Load( "models/ball/ball.vmdl" );
		Components.Get<ModelRenderer>().Tint = Color.Random;
		Components.Create<ModelCollider>().Model = Model.Load( "models/ball/ball.vmdl" );
		Components.Create<Rigidbody>();

		Transform.Scale = Game.Random.Float( 0.5f, 2.0f );
	}

	void ICollisionListener.OnCollisionStart( Collision other )
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
		if ( user.Components.TryGet<PlayerController>( out var player ) )
		{
			// player.Health += 10;

			Destroy();
		}

		return false;
	}
}
