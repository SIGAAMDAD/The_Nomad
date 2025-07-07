using System;
using System.Collections.Generic;
using Godot;
using Renown;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class Bloodbath : Mode {
		private Dictionary<CSteamID, int> Scores = new Dictionary<CSteamID, int>();
		private Dictionary<CSteamID, int> ServerScores = new Dictionary<CSteamID, int>();
		private List<CSteamID> Scoreboard = new List<CSteamID>();
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
				score = Scores[ player.MultiplayerData.Id ]++;
			} else if ( source is NetworkPlayer networkNode && networkNode != null ) {
				score = Scores[ networkNode.MultiplayerData.Id ]++;
			}
			if ( score >= MaxScore ) {

			}
			SendPacket();
		}

		public override void SpawnPlayer( Entity player ) {
		}

		private void SendPacket() {
			byte changedBits = 0;
			int index = 0;
			foreach ( var score in Scores ) {
				// TODO: delta compression
				if ( score.Value != ServerScores[ score.Key ] ) {
					changedBits |= (byte)index;
				}
				index++;
			}
			SyncObject.Write( changedBits );
			foreach ( var score in Scores ) {
				SyncObject.Write( (ulong)score.Key );
				SyncObject.WritePackedInt( score.Value );
			}
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			SyncObject.BeginRead( reader );
			for ( int i = 0; i < Scores.Count; i++ ) {
				Scores[ (CSteamID)SyncObject.ReadUInt64() ] = SyncObject.ReadPackedInt();
			}
		}

		public override void OnPlayerJoined( Entity player ) {
		}
		public override void OnPlayerLeft( Entity player ) {
		}

		public override bool HasTeams() => false;
		public override GameMode GetMode() => GameMode.Bloodbath;

		public override void _Ready() {
			base._Ready();

			MaxScore = Convert.ToInt32( SteamMatchmaking.GetLobbyData( SteamLobby.Instance.GetLobbyID(), "RequiredScore" ) );

			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
		}
    };
};