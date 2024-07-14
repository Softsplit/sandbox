using Sandbox.Diagnostics;
using Scene = Sandbox.Scene;

namespace Softsplit;

public static partial class GameObjectExtensions
{
	/// <summary>
	/// Take damage. Only the host can call this.
	/// </summary>
	/// <param name="go"></param>
	/// <param name="damageInfo"></param>
	public static void TakeDamage( this GameObject go, DamageInfo damageInfo )
	{
		Assert.True( Networking.IsHost );

		foreach ( var damageable in go.Root.Components.GetAll<HealthComponent>() )
		{
			damageable.TakeDamage( damageInfo );
		}
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
