using System;
using System.Collections.Generic;
using Steamworks;

public enum ServerCommandType : uint {
	StartGame,
	EndGame,

	StartCountdown,

	ConnectedToLobby,

	PlayerReady,

	// to start the game
	VoteStart,
	CancelVote,

	StartVoteKick,
	VoteKickRequest,

	VoteKickResponse_Yes,
	VoteKickResponse_No,

	VoteKickResult,

	KickPlayer,

	OwnershipChanged,

	Count
	
	// anything beyond here is reserved for ModeCommands
};

public class ServerCommandManager {
	private static Action<CSteamID>[] CommandCache = new Action<CSteamID>[ (int)ServerCommandType.Count ];
	private static Dictionary<string, Action<CSteamID>> ModeCommands = new Dictionary<string, Action<CSteamID>>();

	public static void SendCommand( ServerCommandType nType ) {
		uint command = (uint)nType;

		byte[] packet = [
			(byte)SteamLobby.MessageType.ServerCommand,
			(byte)( command & 0xff ),
			(byte)( ( command >> 8 ) & 0xff ),
			(byte)( ( command >> 16 ) & 0xff ),
			(byte)( ( command >> 24 ) & 0xff )
		];
		Console.PrintLine( string.Format( "Sending server command: {0}...", nType.ToString() ) );
		SteamLobby.Instance.SendP2PPacket( packet );
	}
	public static void SendCommand( CSteamID targetId, ServerCommandType nType ) {
		uint command = (uint)nType;

		byte[] packet = [
			(byte)SteamLobby.MessageType.ServerCommand,
			(byte)( command & 0xff ),
			(byte)( ( command >> 8 ) & 0xff ),
			(byte)( ( command >> 16 ) & 0xff ),
			(byte)( ( command >> 24 ) & 0xff )
		];

		Console.PrintLine( string.Format( "Sending targeted server command [id:{0}]: {1}...", targetId.ToString(), nType.ToString() ) );
		SteamLobby.Instance.SendTargetPacket( targetId, packet, Constants.k_nSteamNetworkingSend_Reliable );
	}

	public static void ClearModeCommandCache() {
		ModeCommands.Clear();
		Console.PrintLine( "Cleared mode command cache." );
	}
	public static void RegisterCommandType( string name, Action<CSteamID> callback ) {
		ModeCommands.TryAdd( name, callback );
		Console.PrintLine( string.Format( "ServerCommandManager.RegisterCommandType: created new mode command {0}", name ) );
	}

	public static void RegisterCommandCallback( ServerCommandType nType, Action<CSteamID> callback ) {
		CommandCache[ (int)nType ] = callback;
	}
	public static void ExecuteCommand( CSteamID senderId, ServerCommandType nType ) {
		CommandCache[ (int)nType ]?.Invoke( senderId );
	}
};