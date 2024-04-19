using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class ScoreboardEntry : Panel
{
	// public IClient Client;

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

		// if ( !Client.IsValid() )
		//     return;

		if ( TimeSinceUpdate < 0.1f )
			return;

		TimeSinceUpdate = 0;
		UpdateData();
	}

	public virtual void UpdateData()
	{
		/*
		PlayerName.Text = Client.Name;
		Kills.Text = Client.GetInt( "kills" ).ToString();
		Deaths.Text = Client.GetInt( "deaths" ).ToString();
		Ping.Text = Client.Ping.ToString();
		SetClass( "me", Client == Game.LocalClient );
		*/
	}

	/*
	public virtual void UpdateFrom( IClient client )
	{
		Client = client;
		UpdateData();
	}
	*/
}

