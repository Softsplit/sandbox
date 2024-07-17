﻿using Sandbox.Diagnostics;

namespace Softsplit;

public partial class PlayerState : Component
{
	/// <summary>
	/// The player we're currently in the view of (clientside).
	/// Usually the local player, apart from when spectating etc.
	/// </summary>
	public static PlayerState Viewer { get; private set; }

	/// <summary>
	/// Our local player on this client.
	/// </summary>
	public static PlayerState Local { get; private set; }

	// --

	/// <summary>
	/// Who owns this player state?
	/// </summary>
	[HostSync, Property] public ulong SteamId { get; set; }

	/// <summary>
	/// The player's name, which might have to persist if they leave
	/// </summary>
	[HostSync] private string SteamName { get; set; }

	/// <summary>
	/// The connection of this player
	/// </summary>
	public Connection Connection => Network.OwnerConnection;
	public bool IsConnected => Connection is not null && (Connection.IsActive || Connection.IsHost); //smh

	private string name => IsBot ? $"BOT {BotManager.Instance.GetName( BotId )}" : SteamName ?? "";
	/// <summary>
	/// Name of this player
	/// </summary>
	public string DisplayName => $"{name}{(!IsConnected ? " (Disconnected)" : "")}";

	/// <summary>
	/// Are we in the view of this player (clientside)
	/// </summary>
	public bool IsViewer => Viewer == this;

	/// <summary>
	/// Is this the local player for this client
	/// </summary>
	public bool IsLocalPlayer => !IsProxy && !IsBot && Connection == Connection.Local;

	/// <summary>
	/// The main PlayerPawn of this player if one exists, will not change when the player possesses gadgets etc. (synced)
	/// </summary>
	[HostSync, ValidOrNull] public PlayerPawn PlayerPawn { get; set; }

	/// <summary>
	/// The pawn this player is currently in possession of (synced - unless the pawn is not networked)
	/// </summary>
	[Sync] public Pawn Pawn { get; set; }

	public void HostInit()
	{
		// on join, spawn right now if we can
		RespawnState = RespawnState.Immediate;

		SteamId = Connection.SteamId;
		SteamName = Connection.DisplayName;
	}

	[Authority]
	public void ClientInit()
	{
		if ( IsBot )
			return;

		Local = this;
	}

	public void Kick()
	{
		if ( PlayerPawn.IsValid() )
		{
			PlayerPawn.GameObject.Destroy();
		}

		GameObject.Destroy();
		// todo: actually kick em
	}

	public static void OnPossess( Pawn pawn )
	{
		// called from Pawn when one is newly possessed, update Local and Viewer, invoke RPCs for observers

		Local.Pawn = pawn;

		if ( pawn.Network.Active )
		{
			Local.OnNetPossessed();
		}

		Assert.True( pawn.PlayerState.IsValid(), $"Attempted to possess pawn, but pawn '{pawn.DisplayName}' has no attached PlayerState!" );
		Viewer = pawn.PlayerState;
	}

	// sync to other clients what this player is currently possessing
	// Sol: when we track observers we could drop this with an Rpc.FilterInclude?
	[Broadcast]
	private void OnNetPossessed()
	{
		if ( IsViewer && IsProxy )
		{
			Possess();
		}
	}

	public void Possess()
	{
		if ( Pawn is null || IsLocalPlayer )
		{
			// Local player - always assume the controller
			PlayerPawn.Possess();
		}
		else
		{
			// A remote player is possessing this player (spectating)
			// So enter the latest known pawn this player has possessed
			Pawn.Possess();
		}
	}
}
