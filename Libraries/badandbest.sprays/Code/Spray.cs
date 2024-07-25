using Sandbox;
using Sandbox.Utility;

namespace badandbest.Sprays;

/// <summary>
/// A library to allow the placement of sprays in the world
/// </summary>
public static class Spray
{
	[ConVar( "spray", Help = "URL of image. Must be in quotes.", Saved = true )]
	private static string ImageUrl { get; set; }

	private static GameObject LocalSpray;

	/// <summary>
	/// Places an image on a surface.
	/// </summary>
	public static void Place()
	{
		const float RANGE = 128;// Range in GMOD.

		var ray = Game.ActiveScene.Camera.Transform.World.ForwardRay;
		var trace = Game.SceneTrace.Ray( ray, RANGE );

		Place( trace );
	}

	/// <summary>
	/// Places an image on a surface.
	/// </summary>
	/// <param name="trace">The trace to use.</param>
	public static void Place( SceneTrace trace )
	{
		// We only want to hit static bodies. ( maps, etc )
		if ( trace.Run() is not { Body.BodyType: PhysicsBodyType.Static } tr )
			return;

		if ( string.IsNullOrEmpty( ImageUrl ) )
			ImageUrl = "materials/decals/default.png";

		var config = new CloneConfig
		{
			Name = $"Spray - {Steam.PersonaName}",
			Transform = new( tr.HitPosition, Rotation.LookAt( tr.Normal ) ),
			PrefabVariables = new() { { "Image", ImageUrl } }
		};

		LocalSpray?.Destroy();
		LocalSpray = GameObject.Clone( "prefabs/spray.prefab", config );

		LocalSpray.NetworkSpawn();// NetworkSpawn breaks the prefab
		LocalSpray.SetPrefabSource( "prefabs/spray.prefab" );
	}
}

[Title( "Spray Renderer" ), Icon( "imagesearch_roller" )]
internal class SprayRenderer : Renderer
{
	[Property, ImageAssetPath]
	public string Image { get; set; }

	protected override async void OnEnabled()
	{
		var texture = await Texture.LoadAsync( FileSystem.Mounted, Image, false );

		if ( texture is null or { Width: <= 32, Height: <= 32 } )
		{
			// Probably an error texture. Replace with a fallback image.
			Components.Get<DecalRenderer>( FindMode.InChildren ).Material = Material.Load( "materials/fallback.vmat" );
			return;
		}

		var material = Material.Load( "materials/spray.vmat" ).CreateCopy();
		material.Set( "g_tColor", texture );

		Components.Get<DecalRenderer>( FindMode.InChildren ).Material = material;
	}
}
