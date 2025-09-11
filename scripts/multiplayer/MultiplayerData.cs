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

using System.Collections.Generic;
using System.Threading;
using Godot;
using Multiplayer;
using Steamworks;
using Steam;
using ResourceCache;

namespace Multiplayer {
	public partial class MultiplayerData : LevelData {
		private Mode ModeData = null;

		private bool Loaded = false;

		public Mode.GameMode GetMode() {
			return ModeData.GetMode();
		}

		protected override void OnResourcesFinishedLoading() {
			SetProcess( true );

			for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
				if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[ i ] ) || SteamLobby.Instance.LobbyMembers[ i ] == SteamUser.GetSteamID() ) {
					continue;
				}
				NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
				player.MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( SteamLobby.Instance.LobbyMembers[ i ] );
				player.SetOwnerId( SteamLobby.Instance.LobbyMembers[ i ] );
				ModeData.SpawnPlayer( player );
				ModeData.OnPlayerJoined( player );
				Players.Add( SteamLobby.Instance.LobbyMembers[ i ], player );
				PlayerList.AddChild( player );
			}

			Console.PrintLine( "...Finished loading game" );
		}

		protected override void OnPlayerJoined( ulong steamId ) {
			Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

			SteamLobby.Instance.GetLobbyMembers();

			CSteamID userId = (CSteamID)steamId;
			if ( Players.ContainsKey( userId ) ) {
				return;
			}

			NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
			player.MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( userId );
			player.SetOwnerId( userId );
			Players.Add( userId, player );
			PlayerList.AddChild( player );

			ModeData.OnPlayerJoined( player );
			ModeData.SpawnPlayer( player );
		}
		protected override void OnPlayerLeft( ulong steamId ) {
			SteamLobby.Instance.GetLobbyMembers();

			CSteamID userId = (CSteamID)steamId;
			if ( userId == SteamUser.GetSteamID() ) {
				return;
			}

			ModeData.OnPlayerLeft( Players[ userId ] );

			Console.PrintLine(
				string.Format( "{0} has faded away...", ( Players[ userId ] as NetworkPlayer ).MultiplayerData.Username )
			);
			PlayerList.CallDeferred( "remove_child", Players[ userId ] );
			Players[ userId ].QueueFree();
			Players.Remove( userId );
			SteamLobby.Instance.RemovePlayer( userId );
		}

		public override void _Ready() {
			base._Ready();

			Players = new Dictionary<CSteamID, Renown.Entity>();
			PlayerScene = SceneCache.GetScene( "res://scenes/network_player.tscn" );

			ModeData = GetNode<Mode>( "ModeData" );

			ModeData.OnPlayerJoined( ThisPlayer );
			ModeData.SpawnPlayer( ThisPlayer );

			ModeData.Connect( Mode.SignalName.EndGame, Callable.From( () => {
				if ( SteamLobby.Instance.IsHost ) {
					ServerCommandManager.SendCommand( ServerCommandType.EndGame );
				}
				SteamLobby.Instance.LeaveLobby();
				GetTree().ChangeSceneToFile( "res://scenes/main_menu.tscn" );
			} ) );

			ServerCommandManager.RegisterCommandCallback( ServerCommandType.EndGame, ( senderId ) => SteamLobby.Instance.LeaveLobby() );

			Console.PrintLine( string.Format( "Adding {0} members...", SteamLobby.Instance.LobbyMemberCount ) );
			for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
				if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[ i ] ) || SteamLobby.Instance.LobbyMembers[ i ] == SteamUser.GetSteamID() ) {
					continue;
				}
				NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
				player.MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( SteamLobby.Instance.LobbyMembers[ i ] );
				player.SetOwnerId( SteamLobby.Instance.LobbyMembers[ i ] );
				ModeData.OnPlayerJoined( player );
				ModeData.SpawnPlayer( player );
				Players.Add( SteamLobby.Instance.LobbyMembers[ i ], player );
				PlayerList.AddChild( player );
			}

			ServerCommandManager.SendCommand( ServerCommandType.PlayerReady );
		}
	};
};