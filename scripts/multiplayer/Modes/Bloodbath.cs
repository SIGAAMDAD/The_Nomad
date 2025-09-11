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
using Godot;
using Renown;
using Steamworks;
using Steam;

namespace Multiplayer.Modes {
	/*
	===================================================================================
	
	Bloodbath
	
	===================================================================================
	*/
	/// <summary>
	/// Manages data and state relevant to a Bloodbath multiplayer match
	/// </summary>
	
	public partial class Bloodbath : Mode {
		private struct Ranking {
			public CSteamID Id = CSteamID.Nil;
			public int Score = 0;

			public Ranking() {
			}
		};

		/// <summary>
		/// The minimum players required for the mode
		/// </summary>
		public static readonly int MIN_PLAYERS = 1;

		/// <summary>
		/// The maximum amount of players that can play in the mode
		/// </summary>
		public static readonly int MAX_PLAYERS = 16;

		private Dictionary<CSteamID, int> Scoreboard = new Dictionary<CSteamID, int>( SteamLobby.MAX_LOBBY_MEMBERS );
		private Dictionary<CSteamID, int> ServerScores = new Dictionary<CSteamID, int>( SteamLobby.MAX_LOBBY_MEMBERS );

		private int MaxScore = 15;

		private Node2D[] Spawns;

		private NetworkSyncObject SyncObject = new NetworkSyncObject( ( sizeof( ulong ) + sizeof( int ) ) * SteamLobby.MAX_LOBBY_MEMBERS );

		private void OnGameStart() {
			Announcer.Fight();
			LevelData.Instance.ThisPlayer.BlockInput( false );
		}

		private void OnPlayerScore( Entity source, Entity target ) {
			int score = 0;
			if ( source is Player player && player != null ) {
				score = Scoreboard[ player.MultiplayerData.Id ]++;
			} else if ( source is NetworkPlayer networkNode && networkNode != null ) {
				score = Scoreboard[ networkNode.MultiplayerData.Id ]++;
			}
			SendPacket();
			if ( score >= MaxScore ) {
				EmitSignalEndGame();
				EmitSignalShowScoreboard();
				return;
			}
		}

		public override void SpawnPlayer( Entity player ) {
			if ( !SteamLobby.Instance.IsHost ) {
				return;
			}
			player.Die += OnPlayerScore;
		}

		private void SendPacket() {
			ushort changedBits = 0;
			ushort index = 0;
			
			foreach ( var score in Scoreboard ) {
				// TODO: delta compression
				if ( score.Value != ServerScores[ score.Key ] ) {
					changedBits |= index;
				}
				index++;
			}
			SyncObject.Write( changedBits );
			foreach ( var score in Scoreboard ) {
				if ( score.Value != ServerScores[ score.Key ] ) {
					SyncObject.Write( (ulong)score.Key );
					SyncObject.WritePackedInt( score.Value );

					ServerScores[ score.Key ] = score.Value;
				}
			}
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			SyncObject.BeginRead( reader );

			ushort changedBits = SyncObject.ReadUInt16();
			for ( int i = 0; i < Scoreboard.Count; i++ ) {
				if ( ( changedBits & (ushort)i ) != 0 ) {
					Scoreboard[ (CSteamID)SyncObject.ReadUInt64() ] = SyncObject.ReadPackedInt();
				}
			}
		}

		public override void OnPlayerJoined( Entity player ) {
			if ( player is Player self && self != null ) {
				Scoreboard.TryAdd( self.MultiplayerData.Id, 0 );
			} else if ( player is NetworkPlayer networkNode && networkNode != null ) {
				Scoreboard.TryAdd( networkNode.MultiplayerData.Id, 0 );
			}
		}
		public override void OnPlayerLeft( Entity player ) {
			if ( player is Player self && self != null ) {
				Scoreboard.Remove( self.MultiplayerData.Id );
			} else if ( player is NetworkPlayer networkNode && networkNode != null ) {
				Scoreboard.Remove( networkNode.MultiplayerData.Id );
			}
		}

		public override bool HasTeams() => false;
		public override GameMode GetMode() => GameMode.Bloodbath;

		public override void _Ready() {
			base._Ready();

			//			MaxScore = Convert.ToInt32( SteamMatchmaking.GetLobbyData( SteamLobby.Instance.GetLobbyID(), "RequiredScore" ) );

			if ( !SteamLobby.Instance.IsHost ) {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			}
		}
	};
};