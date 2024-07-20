

namespace Softsplit;

public class EntityInfo
{
	public string PrefabPath { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public Texture Icon { get; set; }

	public EntityInfo( string prefabPath, string name, string description, Texture icon )
	{
		PrefabPath = prefabPath;
		Name = name;
		Description = description;
		Icon = icon;
	}
}
