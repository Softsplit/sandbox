public class PreviewModel
{
	public string ModelPath { get; set; }
	public Vector3 PositionOffset { get; set; }
	public Rotation RotationOffset { get; set; }
	public float NormalOffset { get; set; }
	public bool FaceNormal { get; set; }

	GameObject previewObject;

	public void Update( SceneTraceResult trace )
	{
		if ( !trace.Hit || trace.Tags.Contains( "wheel" ) )
		{
			previewObject?.DestroyImmediate();
			previewObject = null;
			return;
		}

		if ( !previewObject.IsValid() )
		{
			previewObject = new GameObject();

			var renderer = previewObject.Components.Create<ModelRenderer>();
			renderer.Tint = Color.White.WithAlpha( 0.5f );
			renderer.Model = Model.Load( ModelPath );

			previewObject.NetworkSpawn();
		}

		previewObject.WorldPosition = trace.HitPosition + PositionOffset + trace.Normal * NormalOffset;
		previewObject.WorldRotation = (FaceNormal ? Rotation.LookAt( trace.Normal ) : Rotation.Identity) * RotationOffset;
	}

	public void Destroy()
	{
		previewObject?.DestroyImmediate();
	}
}
