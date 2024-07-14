public partial class KillFeed : Panel
{
	public static KillFeed Current;

	public KillFeed()
	{
		Current = this;

		StyleSheet.Load( "/LegacyUI/KillFeed/KillFeed.scss" );
	}

	public virtual Panel AddEntry( long lsteamid, string left, long rsteamid, string right, string method )
	{
		var e = Current.AddChild<KillFeedEntry>();

		e.Left.Text = left;
		e.Left.SetClass( "me", lsteamid == (Game.SteamId) );

		e.Method.Text = method;

		e.Right.Text = right;
		e.Right.SetClass( "me", rsteamid == (Game.SteamId) );

		return e;
	}
}
