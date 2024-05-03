/// <summary>
/// Extensions for Surfaces
/// </summary>
public static partial class SandboxBaseExtensions
{
	/// <summary>
	/// Create a footstep effect
	/// </summary>
	public static void DoFootstep( this Surface self, GameObject ent, SceneTraceResult tr, int foot, float volume )
	{
		var sound = foot == 0 ? self.Sounds.FootLeft : self.Sounds.FootRight;

		if ( !string.IsNullOrWhiteSpace( sound ) )
		{
			Sound.Play( sound, tr.EndPosition ).Volume = volume;
		}
		else
		{
			// Give base surface a chance
			self.GetBaseSurface()?.DoFootstep( ent, tr, foot, volume );
		}
	}
}
