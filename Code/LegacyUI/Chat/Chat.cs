public partial class Chat
{
	[Broadcast]
	public static void AddChatEntry( string name, string message, long playerId = 0, bool isInfo = false )
	{
		Current?.AddEntry( name, message, playerId, isInfo );

		// Only log clientside if we're not the listen server host
		/*
		if ( !Game.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
		*/
	}

	public static void Say( string message )
	{
		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		AddChatEntry( Connection.Local.DisplayName, message, Game.SteamId );
	}
}
