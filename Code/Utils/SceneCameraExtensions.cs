namespace Softsplit;

public static class SceneCameraExtensions
{
	public static void FitModel( this SceneCamera camera, SceneObject model )
	{
		var bounds = model.Model.Bounds;
		var max = bounds.Size;
		var radius = MathF.Max( max.x, MathF.Max( max.y, max.z ) );
		var dist = radius / MathF.Sin( camera.FieldOfView.DegreeToRadian() );

		var viewDirection = Vector3.Forward;
		var pos = viewDirection * dist + bounds.Center;

		camera.Position = pos;
		camera.Rotation = global::Rotation.LookAt( bounds.Center - camera.Position ).RotateAroundAxis( -viewDirection, 90 );
	}
}
