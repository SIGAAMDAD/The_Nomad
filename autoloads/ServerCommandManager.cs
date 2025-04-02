using System;
using Steamworks;

public enum ServerCommandType : int {
	StartGame,

	VoteStart,
	CancelVote,

	KickPlayer,
	BanPlayer,

	Count
};

public class ServerCommandManager {
	private static Action<CSteamID>[] CommandCache = new Action<CSteamID>[ (int)ServerCommandType.Count ];

	public static void SendCommand( ServerCommandType nType ) {
		int command = (int)nType;

		byte[] packet = new byte[5];
		packet[0] = (byte)SteamLobby.MessageType.ServerCommand;
		packet[1] = (byte)( command & 0xff );
		packet[2] = (byte)( ( command >> 8 ) & 0xff );
		packet[3] = (byte)( ( command >> 16 ) & 0xff );
		packet[4] = (byte)( ( command >> 24 ) & 0xff );
		SteamLobby.Instance.SendP2PPacket( packet );
	}
	public static void RegisterCommandCallback( ServerCommandType nType, Action<CSteamID> callback ) {
		CommandCache[ (int)nType ] = callback;
	}
	public static void ExecuteCommand( CSteamID senderId, ServerCommandType nType ) {
		CommandCache[ (int)nType ]?.Invoke( senderId );
	}
};