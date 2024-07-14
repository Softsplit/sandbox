using Sandbox.Audio;

namespace Softsplit;

public partial class PlayerVoiceComponent : Voice
{
	[Property] public PlayerState PlayerState { get; set; }

	protected override void OnStart()
	{
		TargetMixer = Mixer.FindMixerByName( "Voice" );
	}
}
