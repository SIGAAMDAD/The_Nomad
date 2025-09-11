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

using Godot;
using System.Collections.Generic;
using Renown;
using Steamworks;
using Steam;

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
			if ( !SteamLobby.Instance.IsHost ) {
				return;
			}

			Members = new List<Entity>();
		}
	};
};