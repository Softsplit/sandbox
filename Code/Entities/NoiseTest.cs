using Sandbox;
using Sandbox.Utility;

[Spawnable]
[Library( "noise_test", Title = "Noise Test" )]
public partial class NoiseTest : Prop
{
	protected override void OnStart()
	{
		Model = Model.Load( "models/citizen_props/balloonregular01.vmdl" );
	}

	protected override void OnUpdate()
	{
		var pos = Transform.Position;
		var right = Transform.Rotation.Right * 4;
		var forward = Transform.Rotation.Forward * 4;
		var up = Transform.Rotation.Up * 50;
		var offset = Time.Now * 20.0f;
		var offsetz = Time.Now;

		var mode = (int)((Time.Now * 0.3f) % 3);

		switch ( mode )
		{
			case 0:
				{
					Gizmo.Draw.Text( "Perlin", Transform.World );
					break;
				}

			case 1:
				{
					Gizmo.Draw.Text( "Fbm", Transform.World );
					break;
				}

			case 2:
				{
					Gizmo.Draw.Text( "Simplex", Transform.World );
					break;
				}
		}


		var size = 100;

		pos -= right * size * 0.5f;
		pos -= forward * size * 0.5f;

		for ( float x = 0; x < size; x++ )
			for ( float y = 0; y < size; y++ )
			{
				float val = 0;

				switch ( mode )
				{
					case 0:
						{
							val = Noise.Perlin( x + offset, y, offsetz );
							break;
						}
					case 1:
						{
							val = Noise.Fbm( 2, x + offset, y, offsetz );
							break;
						}
					case 2:
						{
							val = Noise.Simplex( x + offset, y, offsetz );
							break;
						}
				}

				var start = pos + x * right + y * forward;
				Gizmo.Draw.Line( start, start + up * val );
				Gizmo.Draw.Color = Color.Lerp( Color.Red, Color.Green, val );
			}
	}
}
