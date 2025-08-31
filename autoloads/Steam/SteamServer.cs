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

using Steam.Server;
using System.Collections.Generic;
using Renown;

namespace Steam {
	public enum ServerState : byte {
		Waiting,    // no map loaded, in waiting room
		Loading,    // loading the map, waiting for sync between all clients
		Active,     // running
	};

	/*
	===================================================================================

	SteamServer

	handles all server and lobby associated logic

	===================================================================================
	*/

	public class SteamServer {
		public int MaxMembers { get; private set; } = 0;
		public uint LobbyGameMode { get; private set; } = 0;
		public string LobbyName { get; private set; }
		public string LobbyMap { get; private set; }
		public SteamLobby.Visibility LobbyVisibility { get; private set; } = SteamLobby.Visibility.Public;

		private ServerState State = ServerState.Waiting;
		private Dictionary<int, Entity> EntityManager = new Dictionary<int, Entity>();

		private System.Threading.Thread WorkThread;

		public SteamServer( string name, string map, uint gameMode, int maxPlayers ) {
			if ( maxPlayers < 1 || maxPlayers > SteamLobby.MAX_LOBBY_MEMBERS ) {
				Console.PrintError( $"SteamServer.Init: invalid MaxPlayers {maxPlayers}, canceling creation" );
				return;
			} else if ( gameMode < (uint)Multiplayer.Mode.GameMode.Bloodbath || gameMode >= (uint)Multiplayer.Mode.GameMode.Count ) {
				Console.PrintError( $"SteamServer.Init: invalid GameMode \"{gameMode}\", canceling creation" );
				return;
			} else if ( map.Length == 0 || !MultiplayerMapManager.IsMapValid( map ) ) {
				Console.PrintError( $"SteamServer.Init: invalid Map \"{map}\", canceling creation" );
				return;
			}

			LobbyName = name;
			LobbyMap = map;
			LobbyGameMode = gameMode;
			MaxMembers = maxPlayers;

			WorkThread = new System.Threading.Thread( () => {

			} );
		}

		/*
		===============
		SendClientSnapshot
		===============
		*/
		public void SendClientSnapshot( ref Client client ) {

		}

		/*
		===============
		Update
		===============
		*/
		public void Update() {
		}
	};
};