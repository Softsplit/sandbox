namespace Softsplit.Ents
{
	using Sandbox;
	class Balloon : Component
	{
		protected override void OnAwake()
		{
			base.OnAwake();
			Components.GetInChildrenOrSelf<ModelRenderer>().Tint = Color.Random;
		}
		protected override void OnUpdate()
		{
			if ( !Components.GetInChildrenOrSelf<Rigidbody>().MotionEnabled )
				return;
			Vector3 velocity = Components.GetInChildrenOrSelf<Rigidbody>().Velocity;
			velocity = Vector3.Lerp( velocity, Vector3.Up * 150f, 0.4f );
			Components.GetInChildrenOrSelf<Rigidbody>().Velocity = velocity;
		}
	}
}
