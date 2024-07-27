


global using Sandbox.UI.Construct;
global using Editor;
global using System;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using Sandbox.Network;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;
global using System.Numerics;
global using System.Globalization;
global using Sandbox.Utility;
using System.Runtime.CompilerServices;
using static Sandbox.Clothing;

namespace Softsplit;


public class SpawnableEntity : Component
{

	protected override void OnStart()
	{
		Network.DropOwnership();
	}
	
	/// <summary>
	/// The catagory of the item.
	/// </summary>
	//[Sync]
	//[Property]
	//public string Catagory { get; set; }


	/// <summary>
	/// The name of the item.
	/// </summary>
	[Sync]
	[Property]
	public string Name { get; set; }

	/// <summary>
	/// The icon to display.
	/// </summary>
	[Property] public IconSettings Icon { get; set; }

	/// <summary>
	/// The description of the item.
	/// </summary>
	[Property] public string Description { get; set; }
	[Sync] public string Prefab { get; private set; }

	public Texture IconTexture => Texture.Load( FileSystem.Mounted, Icon.Path );

	public static implicit operator SpawnableEntity( GameObject obj )
		=> obj.Components.Get<SpawnableEntity>();

}
