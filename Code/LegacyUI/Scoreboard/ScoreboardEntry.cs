public partial class ScoreboardEntry : Panel
{
	public Connection Client;

	public Label PlayerName;
	public Label Kills;
	public Label Deaths;
	public Label Ping;

	public ScoreboardEntry()
	{
		AddClass( "entry" );

		PlayerName = Add.Label( "PlayerName", "name" );
		Kills = Add.Label( "", "kills" );
		Deaths = Add.Label( "", "deaths" );
		Ping = Add.Label( "", "ping" );
	}

	RealTimeSince TimeSinceUpdate = 0;

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		if ( TimeSinceUpdate < 0.1f )
			return;

		TimeSinceUpdate = 0;
		UpdateData();
	}

	public virtual void UpdateData()
	{
		PlayerName.Text = Client.DisplayName;
		SetClass( "me", Client == Connection.Local );
		// Kills.Text = Client.GetUserData( "kills" ).ToString();
		// Deaths.Text = Client.GetUserData( "deaths" ).ToString();
		Ping.Text = Client.Ping.CeilToInt().ToString();
	}

	public virtual void UpdateFrom( Connection client )
	{
		Client = client;
		UpdateData();
	}
}

