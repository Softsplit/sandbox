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

	[ConCmd( "sandbox_say" ), Authority]
	public static void Say( string message )
	{
		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		Log.Info( $"{Rpc.Caller}: {message}" );
		AddChatEntry( Rpc.Caller.DisplayName, message, (long)Rpc.Caller.SteamId );
	}

	/*
	[ConCmd.Server( "sandbox_say" )]
	public static void Say( string message )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );
		AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message, ConsoleSystem.Caller.SteamId );
	}
	*/
}
