using System.Collections.Generic;
using Renown;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class Bloodbath : Mode {
		private Dictionary<CSteamID, int> Scores = new Dictionary<CSteamID, int>();
		private List<CSteamID> Scoreboard = new List<CSteamID>();
		private Player ThisPlayer;

		private NetworkWriter SyncObject = new NetworkWriter( ( sizeof( ulong ) + sizeof( int ) ) * SteamLobby.MAX_LOBBY_MEMBERS );

		private void OnGameStart() {
			Announcer.Fight();
			ThisPlayer.BlockInput( false );
		}

		private void OnPlayerScore( Entity source, Entity target ) {
			if ( source is Player player && player != null ) {
				Scores[ player.MultiplayerData.Id ]++;
			} else if ( source is NetworkPlayer networkNode && networkNode != null ) {
				Scores[ networkNode.MultiplayerData.Id ]++;
			}
			SendPacket();
		}

		public override void SpawnPlayer(Entity player) {
			base.SpawnPlayer(player);
		}

		private void SendPacket() {
			foreach ( var score in Scores ) {
				SyncObject.Write( (ulong)score.Key );
				SyncObject.Write( score.Value );
			}
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			for ( int i = 0; i < Scores.Count; i++ ) {
				Scores[ (CSteamID)reader.ReadUInt64() ] = reader.ReadInt32();
			}
		}

		public override void _Ready() {
		    base._Ready();

			SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
		}
    };
};