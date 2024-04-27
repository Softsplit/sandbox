[Spawnable]
[Library( "ent_balloon", Title = "Balloon" )]
public partial class BalloonEntity : Prop
{
	private static float GravityScale => -0.2f;

	protected override void OnStart()
	{
		Model = Model.Load( "models/citizen_props/balloonregular01.vmdl" );
		Components.Get<ModelRenderer>().Tint = Color.Random;
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
