/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 3 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using Godot;
using Steamworks;
using System;

namespace Steam.sLobbySystem {
	public class GameServerManager {
		private bool Initialized;

		private ushort SERVER_PORT = 27015;
		private ushort QUERY_PORT = 27016;

		public GameServerManager() {
			if ( !Initialized ) {
				InitializeGameServer();
			}
		}

		public void Shutdown() {
			SteamGameServer.LogOff();
			GameServer.Shutdown();
		}

		private void InitializeGameServer() {
			try {
				GameServer.Init( 0, SERVER_PORT, QUERY_PORT, EServerMode.eServerModeAuthenticationAndSecure, "1.0.0.0" );

				SteamGameServer.SetModDir( "The Nomad" );
				SteamGameServer.SetProduct( "The Nomad" );
				SteamGameServer.SetGameDescription( "2D Action/Adventure RPG" );

				SteamGameServer.SetDedicatedServer( true );
				SteamGameServer.SetMaxPlayerCount( SteamLobby.MAX_LOBBY_MEMBERS );
				SteamGameServer.SetPasswordProtected( false );
				SteamGameServer.SetMapName( SteamLobby.Instance.LobbyMap );
				SteamGameServer.LogOnAnonymous();

				Callback<SteamServersConnected_t>.Create( OnSteamServersConnected );
//				Callback<SteamServerConnectFailure_t>.Create( OnSteamServersConnectFailure );
//				Callback<SteamServersDisconnected_t>.Create( OnSteamServersDisconnected );
				Callback<ValidateAuthTicketResponse_t>.Create( OnValidateAuthTicketResponse );

				Initialized = true;
			} catch ( Exception e ) {
				Console.PrintError( string.Format( "[STEAM] Exception creating GameServer: {0}", e.Message ) );
			}
		}

		private void OnSteamServersConnected( SteamServersConnected_t pCallback ) {
			Console.PrintLine( "[STEAM] GameServer backend connected" );
			SteamGameServer.SetAdvertiseServerActive( true );
		}
		private void OnValidateAuthTicketResponse( ValidateAuthTicketResponse_t pCallback ) {
			CSteamID steamID = pCallback.m_SteamID;
			EAuthSessionResponse response = pCallback.m_eAuthSessionResponse;

			if ( response == EAuthSessionResponse.k_EAuthSessionResponseOK ) {
				Console.PrintLine( string.Format( "[STEAM] Player {0} authenticated.", steamID ) );
			} else {
				Console.PrintLine( string.Format( "[STEAM] Player {0} not authenticated.", steamID ) );
				SteamGameServer.EndAuthSession( steamID );
			}
		}
	};
};