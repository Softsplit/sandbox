namespace Softsplit;

/// <summary>
/// Handles the main game loop.
/// </summary>
public sealed partial class GameMode : SingletonComponent<GameMode>, Component.INetworkListener
{
	private TimeSince _sinceLastSoundHandleLog;

	protected override void OnStart()
	{
		base.OnStart();

		GameUtils.LogPlayers();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( _sinceLastSoundHandleLog > 5f )
		{
			_sinceLastSoundHandleLog = 0f;

			var list = new List<SoundHandle>();
			SoundHandle.GetActive( list );

			var mostCommon = list
				.GroupBy( x => x.Name )
				.Select( x => (Name: x.Key, Count: x.Count()) )
				.OrderByDescending( x => x.Count )
				.FirstOrDefault();

			Log.Info( $"Active sound handle count: {list.Count}, most common: {mostCommon.Name} ({mostCommon.Count})" );
		}
	}

	void INetworkListener.OnBecameHost( Connection previousHost )
	{
		Log.Info( "We became the host, taking over the game loop..." );

		GameUtils.LogPlayers();
	}

	private readonly Dictionary<Type, Component> _componentCache = new();

	/// <summary>
	/// Gets the given component from within the game mode's object hierarchy, or null if not found / enabled.
	/// </summary>
	public T Get<T>( bool required = false )
		where T : class
	{
		if ( !_componentCache.TryGetValue( typeof( T ), out var component ) || component is { IsValid: false } || component is { Active: false } )
		{
			component = Components.GetInDescendantsOrSelf<T>() as Component;
			_componentCache[typeof( T )] = component;
		}

		if ( required && component is not T )
		{
			throw new Exception( $"Expected a {typeof( T ).Name} to be active in the {nameof( GameMode )}!" );
		}

		return component as T;
	}
}
