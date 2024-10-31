public static partial class GameObjectExtensions
{
	/// <summary>
	/// Creates a <see cref="TimedDestroyComponent"/> which will deferred delete the <see cref="GameObject"/>.
	/// </summary>
	/// <param name="self"></param>
	/// <param name="seconds"></param>
	public static void DestroyAsync( this GameObject self, float seconds = 1.0f )
	{
		var component = self.Components.Create<TimedDestroyComponent>();
		component.Time = seconds;
	}
}
