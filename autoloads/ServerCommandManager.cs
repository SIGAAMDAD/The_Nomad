using System;
using Steamworks;

public enum ServerCommandType : uint {
	StartGame,

	VoteStart,
	CancelVote,

	KickPlayer,

	Count
};

public class ServerCommandManager {
	private static Action<CSteamID>[] CommandCache = new Action<CSteamID>[ (int)ServerCommandType.Count ];

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
		SteamLobby.Instance.SendTargetPacket( targetId, packet );
	}

	public static void RegisterCommandCallback( ServerCommandType nType, Action<CSteamID> callback ) {
		CommandCache[ (int)nType ] = callback;
	}
	public static void ExecuteCommand( CSteamID senderId, ServerCommandType nType ) {
		CommandCache[ (int)nType ]?.Invoke( senderId );
	}
};