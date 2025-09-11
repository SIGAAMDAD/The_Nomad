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
using Renown;
using Steam;

namespace Multiplayer.Objectives {
	public partial class Hill : Area2D {
		/// <summary>
		/// team that is currently holding the hill
		/// </summary>
		private Modes.Team CurrentTeam;
		private Modes.Team ContestingTeam;

		private float[] Scores;

		private void OnHillShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is Entity entity && entity != null && entity.HasMeta( "Team" ) ) {
				SetProcess( true );

				Modes.Team team = (Modes.Team)entity.GetMeta( "Team" );

				if ( CurrentTeam != null ) {
					ContestingTeam = team;
				} else {
					CurrentTeam = team;
				}
			}
		}
		private void OnHillShapeExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			Godot.Collections.Array<Node2D> entities = GetOverlappingBodies();
			if ( entities.Count > 0 ) {
				SetProcess( true );

				int currentTeam = 0;
				int contestingTeam = 0;
				for ( int i = 0; i < entities.Count; i++ ) {
					if ( entities[ i ].HasMeta( "Team" ) && entities[ i ].GetMeta( "Team" ).AsGodotObject() is Modes.Team team && team != null ) {
						if ( team == CurrentTeam ) {
							currentTeam++;
						} else {
							contestingTeam++;
						}
					}
				}
				if ( currentTeam == 0 ) {
					// transfer power
					CurrentTeam = ContestingTeam;
					ContestingTeam = null;
				}
			} else {
				SetProcess( false );
			}
		}

		public override void _Ready() {
			base._Ready();

			if ( !SteamLobby.Instance.IsHost ) {
				return;
			}

			Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnHillShapeEntered ) );
			Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnHillShapeExited ) );
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			if ( CurrentTeam != null ) {
				if ( ContestingTeam == null ) {
					Scores[ CurrentTeam.GetTeamIndex() ] += 1.0f * (float)delta;
				}
			}
		}
	};
};