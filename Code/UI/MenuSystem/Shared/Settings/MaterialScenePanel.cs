using Sandbox.UI;

public class MaterialScenePanel : ScenePanel
{
	public Material Material { get; set; }

	private SceneModel _so;

	public override void Tick()
	{
		if ( _so is null )
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		World = new SceneWorld();
		_so = new SceneModel( World, "models/dev/plane.vmdl", Transform.Zero );
		_so.Position = new Vector3( 300f, 0, 0f );
		_so.Rotation = new Angles( 90f, 180f, 0f );
		_ = new SceneDirectionalLight( World, Rotation.From( 45, -45, 45 ), Color.White );
		if ( Material is not null )
		{
			_so.SetMaterialOverride( Material );
		}
		Camera.BackgroundColor = Color.Transparent;
		Camera.FieldOfView = 30.0f;
		Camera.Rotation = Rotation.LookAt( Vector3.Forward );
		Camera.ZFar = 15000.0f;
		Camera.Ortho = false;
		Camera.AmbientLightColor = Color.White * 0.05f;
		RenderOnce = true;
		RenderNextFrame();
	}
}
