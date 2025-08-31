/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System;
using System.Collections.Generic;
using Steamworks;
using Steam;

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

/*
===================================================================================

ServerCommandManager

===================================================================================
*/

public class ServerCommandManager {
	private static Action<CSteamID>[] CommandCache = new Action<CSteamID>[ (int)ServerCommandType.Count ];
	private static Dictionary<string, Action<CSteamID>> ModeCommands = new Dictionary<string, Action<CSteamID>>();

	/*
	===============
	SendCommand
	===============
	*/
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

	/*
	===============
	SendCommand
	===============
	*/
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

	/*
	===============
	ClearModeCommandCache
	===============
	*/
	public static void ClearModeCommandCache() {
		ModeCommands.Clear();
		Console.PrintLine( "Cleared mode command cache." );
	}

	/*
	===============
	RegisterCommandType
	===============
	*/
	public static void RegisterCommandType( string name, Action<CSteamID> callback ) {
		ModeCommands.TryAdd( name, callback );
		Console.PrintLine( string.Format( "ServerCommandManager.RegisterCommandType: created new mode command {0}", name ) );
	}

	/*
	===============
	RegisterCommandCallback
	===============
	*/
	public static void RegisterCommandCallback( ServerCommandType nType, Action<CSteamID> callback ) {
		CommandCache[ (int)nType ] = callback;
	}

	/*
	===============
	ExecuteCommand
	===============
	*/
	public static void ExecuteCommand( CSteamID senderId, ServerCommandType nType ) {
		CommandCache[ (int)nType ]?.Invoke( senderId );
	}
};