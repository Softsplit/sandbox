namespace Softsplit;

public sealed class PrefabInitializer : Component, Component.ExecuteInEditor
{
	protected override void OnAwake() => PrefabLibrary.Initialize();
}
