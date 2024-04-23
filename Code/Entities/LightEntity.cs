using Sandbox;

[Spawnable]
[Library( "ent_light", Title = "Light" )]
public partial class LightEntity : PointLight // IUse
{
	protected override void OnStart()
	{
		Components.Create<ModelRenderer>().Model = Model.Load( "models/light/light_tubular.vmdl" );
		Components.Create<ModelCollider>().Model = Model.Load( "models/light/light_tubular.vmdl" );
		Components.Create<Rigidbody>();

		Tags.Add( "solid" );
	}

	/*
	public bool IsUsable( GameObject user )
	{
		return true;
	}

	public bool OnUse( GameObject user )
	{
		Enabled = !Enabled;

		Sound.Play( Enabled ? "flashlight-on" : "flashlight-off" );

		return false;
	}
	*/
}
