using Godot;
using System.Collections.Generic;
using Renown;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class Team : Node {
		[Export]
		private Spawner[] Spawns;
		[Export]
		private int Index;

		private List<Entity> Members;
		private NetworkSyncObject SyncObject = new NetworkSyncObject( 24 );

		public int GetTeamIndex() => Index;

		private void SendSpawnCommand( Entity player, Spawner spawn ) {
			SyncObject.Write( (byte)SteamLobby.MessageType.ServerCommand );

			CSteamID target = CSteamID.Nil;
			if ( player is Player self && self != null ) {
				target = self.MultiplayerData.Id;
			} else if ( player is NetworkPlayer networkPlayer && networkPlayer != null ) {
				target = networkPlayer.MultiplayerData.Id;
			}

			spawn.Use();

			SyncObject.Write( (byte)SteamLobby.Instance.GetMemberIndex( target ) );
			SyncObject.Write( (byte)PlayerUpdateType.SetSpawn );
			SyncObject.Write( spawn.GlobalPosition );
			SyncObject.Sync( target );
		}

		private Spawner GetSpawn() {
			for ( int i = 0; i < Spawns.Length; i++ ) {
				if ( !Spawns[ i ].IsUsed() ) {
					return Spawns[ i ];
				}
			}
			return Spawns[ 0 ];
		}
		private void SpawnPlayer( Entity source, Entity target ) {
			Spawner spawn = GetSpawn();

			CSteamID syncTarget = CSteamID.Nil;
			if ( source is Player self && self != null ) {
				self.MultiplayerData.Kills++;
			} else if ( source is NetworkPlayer networkPlayer && networkPlayer != null ) {
				networkPlayer.MultiplayerData.Kills++;
			}
		}

		public void AddPlayer( Entity player ) {
			Console.PrintLine( string.Format( "[SERVER] Added player to team " + Name ) );
			player.SetMeta( "Team", this );
			Members.Add( player );
		}

		public int GetMemberCount() => Members.Count;

		public override void _Ready() {
			base._Ready();

			// only the host runs the team logic
			if ( !SteamLobby.Instance.IsOwner() ) {
				return;
			}

			Members = new List<Entity>();
		}
	};
};