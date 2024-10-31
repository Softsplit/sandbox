/// <summary>
/// A simple component that destroys its GameObject.
/// </summary>
public sealed class TimedDestroyComponent : Component
{
	/// <summary>
	/// How long until we destroy the GameObject.
	/// </summary>
	[Property] public float Time { get; set; } = 1f;

	/// <summary>
	/// The real time until we destroy the GameObject.
	/// </summary>
	[Property, ReadOnly] TimeUntil TimeUntilDestroy { get; set; } = 0;

	protected override void OnStart()
	{
		TimeUntilDestroy = Time;
	}

	protected override void OnUpdate()
	{
		if ( TimeUntilDestroy )
		{
			GameObject.Destroy();
		}
	}
}
