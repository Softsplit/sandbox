public static partial class GameObjectExtensions
{
	public static async void DestroyAsync( this GameObject go, float time )
	{
		await Task.Delay( (int)(time * 1000.0f) );
		go.Destroy();
	}

	public static void CopyPropertiesTo( this Component src, Component dst )
	{
		var json = src.Serialize().AsObject();
		json.Remove( "__guid" );
		dst.DeserializeImmediately( json );
	}

	public static string GetScenePath( this GameObject go )
	{
		return go is Scene ? "" : $"{go.Parent.GetScenePath()}/{go.Name}";
	}
}
