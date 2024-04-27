[Spawnable]
[Library( "ent_lamp", Title = "Lamp" )]
public partial class LampEntity : SpotLight, IUse
{
	protected override void OnStart()
	{
		Components.Create<ModelRenderer>().Model = Model.Load( "models/torch/torch.vmdl" );
		Components.Create<ModelCollider>().Model = Model.Load( "models/torch/torch.vmdl" );
		Components.Create<Rigidbody>();

		Tags.Add( "solid" );
	}

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
}
