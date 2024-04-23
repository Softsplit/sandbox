using Sandbox;

[Spawnable]
[Library( "ent_balloon", Title = "Balloon" )]
public partial class BalloonEntity : Component
{
	private static float GravityScale => -0.2f;

	protected override void OnStart()
	{
		Components.Create<ModelRenderer>().Model = Model.Load( "models/citizen_props/balloonregular01.vmdl" );
		Components.Get<ModelRenderer>().Tint = Color.Random;
		Components.Create<ModelCollider>().Model = Model.Load( "models/citizen_props/balloonregular01.vmdl" );
		Components.Create<Rigidbody>();
	}

	protected override void OnUpdate()
	{
		Components.Get<Rigidbody>().PhysicsBody.GravityScale = GravityScale;
	}

	protected override void OnDestroy()
	{
		Sound.Play( "balloon_pop_cute" );
	}
}
