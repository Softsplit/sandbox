namespace Softsplit;

public partial class PlayerState : Component.INetworkListener
{
	/// <summary>
	/// How long has it been since this player has d/c'd
	/// </summary>
	RealTimeSince TimeSinceDisconnected { get; set; }

	/// <summary>
	/// How long does it take to clean up a player once they disconnect?
	/// </summary>
	public static float DisconnectCleanupTime { get; set; } = 0;

	void INetworkListener.OnDisconnected( Connection channel )
	{
		if ( Connection == channel )
		{
			TimeSinceDisconnected = 0;
		}
	}

	protected override void OnUpdate()
	{
		CheckPropUndo();
		
		if ( IsConnected ) return;
		if ( IsProxy ) return;

		if ( TimeSinceDisconnected > DisconnectCleanupTime )
		{
			GameObject.Destroy();
		}
	}
}
