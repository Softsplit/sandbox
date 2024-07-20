
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace Softsplit;

/// <summary>
/// A simple structure for a prefab component. 
/// Used for getting parameter values without having to instantiate a prefab.
/// </summary>
public class ComponentDefinition
{
	public TypeDescription Type;
	public JsonObject Object;
	public PrefabDefinition Prefab;

	private static JsonSerializerOptions options = new()
	{
		Converters = {
			new JsonStringEnumConverter(), // Retarded hack to convert enum names to actual enums!!
		}
	};

	/// <summary>
	/// Gets the value of property with type T by key.
	/// </summary>
	/// <param name="key"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T Get<T>( string key )
	{
		if ( Object == null
		 || !Object.TryGetPropertyValue( key, out var node )
		 || node == null ) return default;

		// Deserialize classes & structs, otherwise we can just get the value as type T.
		var type = TypeLibrary.GetType<T>();
		if ( type.IsValueType || type.IsClass || type.IsEnum )
			return node.Deserialize<T>( options );

		return node.GetValue<T>();
	}
}

/// <summary>
/// A simple structure of relevant prefab information.
/// </summary>
public class PrefabDefinition
{
	public string Name;
	public PrefabFile Prefab;
	public string Path => Prefab.ResourcePath;
	public IReadOnlyList<ComponentDefinition> Components;

	/// <summary>
	/// Gets all components of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public IEnumerable<ComponentDefinition> GetComponents<T>() where T : Component
		=> Components.Where( component => component.Type?.TargetType.IsAssignableTo( typeof( T ) ) ?? false );

	/// <summary>
	/// Gets the first component of type T.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public ComponentDefinition GetComponent<T>() where T : Component
		=> Components.FirstOrDefault( component => component.Type?.TargetType.IsAssignableTo( typeof( T ) ) ?? false );
}

/// <summary>
/// Helper class for retrieving data from prefabs and their components.
/// </summary>
public static class PrefabLibrary
{
	public static IReadOnlyDictionary<PrefabFile, PrefabDefinition> All => all;
	private static Dictionary<PrefabFile, PrefabDefinition> all;

	public static void Initialize()
	{
		all = ResourceLibrary.GetAll<PrefabFile>()
			.Select( prefab =>
			{
				var root = prefab.RootObject?.AsObject();
				if ( root == null )
					return null;

				var components = new List<ComponentDefinition>();
				var name = root["Name"]?.GetValue<string>();

				// Go through all of the prefab's components.
				var rootComponents = prefab.RootObject?["Components"]?.AsArray();
				if ( rootComponents == null )
					return null;

				var def = new PrefabDefinition
				{
					Name = name,
					Prefab = prefab,
					Components = components
				};

				foreach ( var component in rootComponents )
				{
					var obj = component.AsObject();
					var typeName = obj?["__type"]?.GetValue<string>();
					if ( typeName == null )
						continue;

					components.Add( new()
					{
						Type = TypeLibrary.GetType( typeName ),
						Object = obj,
						Prefab = def
					} );
				}

				// Return a PrefabDefinition with all relevant data.
				return def;
			} )
			.Where( p => p != null )
			.ToDictionary( p => p.Prefab, p => p );
	}

	/// <summary>
	/// Find all prefabs that contain a component.
	/// This is probably slow so avoid using it too much.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static IEnumerable<PrefabDefinition> FindByComponent<T>() where T : Component
		=> all
			.Where( kvp => kvp.Value.Components.Any( component => component?.Type?.TargetType.IsAssignableTo( typeof( T ) ) ?? false ) )
			.Select( kvp => kvp.Value );

	/// <summary>
	/// Converts PrefabFile to a PrefabDefinition.
	/// </summary>
	/// <param name="resource"></param>
	/// <returns></returns>
	public static PrefabDefinition AsDefinition( this PrefabFile resource )
	{
		if ( all.TryGetValue( resource, out var definition ) )
			return definition;

		return null;
	}

	/// <summary>
	/// Tries to find a PrefabDefinition by Prefab path.
	/// </summary>
	/// <param name="path"></param>
	/// <param name="prefab"></param>
	/// <returns></returns>
	public static bool TryGetByPath( string path, out PrefabDefinition prefab )
		=> (prefab = all.FirstOrDefault( kvp => FileSystem.NormalizeFilename( path ).Equals( kvp.Value?.Path ) ).Value) != null;
}
