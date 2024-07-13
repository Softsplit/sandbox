using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class Scoreboard<T> : Panel where T : ScoreboardEntry, new()
{
	public Panel Canvas { get; protected set; }
	Dictionary<Connection, T> Rows = new();

	public Panel Header { get; protected set; }

	public Scoreboard()
	{
		StyleSheet.Load( "/LegacyUI/Scoreboard/Scoreboard.scss" );
		AddClass( "scoreboard" );

		AddHeader();

		Canvas = Add.Panel( "canvas" );
	}

	public override void Tick()
	{
		SetClass( "open", ShouldBeOpen() );

		if ( !IsVisible )
			return;

		//
		// Clients that were added
		//
		foreach ( var client in Connection.All.Except( Rows.Keys ) )
		{
			var entry = AddClient( client );
			Rows[client] = entry;
		}

		foreach ( var client in Rows.Keys.Except( Connection.All ) )
		{
			if ( Rows.TryGetValue( client, out var row ) )
			{
				row?.Delete();
				Rows.Remove( client );
			}
		}
	}

	public virtual bool ShouldBeOpen()
	{
		return Input.Down( "score" );
	}

	protected virtual void AddHeader()
	{
		Header = Add.Panel( "header" );
		Header.Add.Label( "Name", "name" );
		Header.Add.Label( "Kills", "kills" );
		Header.Add.Label( "Deaths", "deaths" );
		Header.Add.Label( "Ping", "ping" );
	}

	protected virtual T AddClient( Connection entry )
	{
		var p = Canvas.AddChild<T>();
		p.Client = entry;
		return p;
	}
}
