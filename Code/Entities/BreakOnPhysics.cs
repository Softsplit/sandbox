
namespace Softsplit
{
	class BreakOnPhysics : Component, Component.ICollisionListener
	{
		public Rigidbody body;
		public Prop prop;
		private Vector3 PrevVelocity;
		protected override void OnStart()
		{
			body = Components.Get<Rigidbody>();
			prop = Components.Get<Prop>();
			PrevVelocity = body.Velocity;
		}
		protected override void OnFixedUpdate()
		{
			PrevVelocity = body.Velocity;
		}
		public void OnCollisionStart( Collision collision )
		{
			Log.Info( PrevVelocity.Length );
			if ( PrevVelocity.Length > 1900 || collision.Contact.Speed.Length > 1900 )
			{
				Sandbox.DamageInfo damage = new Sandbox.DamageInfo( 100, this.GameObject, this.GameObject );
				prop.OnDamage( damage );
			}
		}/*
		public void OnCollisionStop( CollisionStop collision )
		{
		}*/
	}
}
