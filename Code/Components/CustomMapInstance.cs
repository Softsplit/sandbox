public sealed class CustomMapInstance : MapInstance
{
	protected override void OnCreateObject( GameObject go, MapLoader.ObjectEntry kv )
	{
		base.OnCreateObject( go, kv );

		if ( kv.TypeName.StartsWith( "prop" ) ) go.AddComponent<PropHelper>();
	}
}
