using System;
using System.Collections.Generic;
using Godot;
using Renown;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class Bloodbath : Mode {
		private struct Ranking {
			public CSteamID Id = CSteamID.Nil;
			public int Score = 0;

			public Ranking() {
			}
		};

		public static readonly int MinPlayers = 1;
		public static readonly int MaxPlayers = 16;

		private Dictionary<CSteamID, int> Scoreboard = new Dictionary<CSteamID, int>( SteamLobby.MAX_LOBBY_MEMBERS );
		private Dictionary<CSteamID, int> ServerScores = new Dictionary<CSteamID, int>( SteamLobby.MAX_LOBBY_MEMBERS );
		private Player ThisPlayer;

		private int MaxScore = 15;

		private Node2D[] Spawns;

		private NetworkSyncObject SyncObject = new NetworkSyncObject( ( sizeof( ulong ) + sizeof( int ) ) * SteamLobby.MAX_LOBBY_MEMBERS );

		private void OnGameStart() {
			Announcer.Fight();
			ThisPlayer.BlockInput( false );
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
			if ( !SteamLobby.Instance.IsOwner() ) {
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

			if ( !SteamLobby.Instance.IsOwner() ) {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			}
		}
    };
};