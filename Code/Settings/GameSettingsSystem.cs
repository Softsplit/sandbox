using Sandbox.Audio;

namespace Softsplit;

public class GameSettings
{
	[Title( "Field Of View" ), Description( "Effects the camera's vision." ), Group( "Game" ), Icon( "grid_view" ), Range( 65, 110, 1 )]
	public float FieldOfView { get; set; } = 85;

	[Title( "Master" ), Description( "The overall volume" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float MasterVolume { get; set; } = 100;

	[Title( "SFX" ), Description( "Most effects in the game" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float SFXVolume { get; set; } = 100;

	[Title( "UI" ), Description( "interface sounds" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float UIVolume { get; set; } = 100;

	[Title( "Voice" ), Description( "" ), Group( "Volume" ), Icon( "grid_view" ), Range( 0, 100, 5 )]
	public float VoiceVolume { get; set; } = 100;

	[Title( "View Bob" ), Group( "Game" ), Range( 0, 100, 5f )]
	public float ViewBob { get; set; } = 100f;

	/*
	[Title( "Show Dot" ), Group( "Crosshair" )]
	public bool ShowCrosshairDot { get; set; } = true;

	[Title( "Dynamic" ), Group( "Crosshair" )]
	public bool DynamicCrosshair { get; set; } = true;

	[Title( "Crosshair Length" ), Group( "Crosshair" ), Range( 2, 50, 1 )]
	public float CrosshairLength { get; set; } = 15;

	[Title( "Crosshair Distance" ), Group( "Crosshair" ), Range( 2, 50, 1 )]
	public float CrosshairDistance { get; set; } = 15;

	[Title( "Crosshair Color" ), Group( "Crosshair" )]
	public Color CrosshairColor { get; set; } = Color.White;
	*/
}

public partial class GameSettingsSystem
{
	private static GameSettings current { get; set; }
	public static GameSettings Current
	{
		get
		{
			if ( current is null ) Load();
			return current;
		}
		set
		{
			current = value;
		}
	}

	public static string FilePath => "gamesettings.json";

	public static void Save()
	{
		Mixer.Master.Volume = Current.MasterVolume / 100;
		var channel = Mixer.Master.GetChildren();
		channel[0].Volume = Current.SFXVolume / 100;
		channel[1].Volume = Current.UIVolume / 100;
		channel[2].Volume = Current.VoiceVolume / 100;

		FileSystem.Data.WriteJson( FilePath, Current );
	}

	public static void Load()
	{
		Current = FileSystem.Data.ReadJson<GameSettings>( FilePath, new() );
	}
}
