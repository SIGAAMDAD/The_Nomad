using Godot;
using Renown;

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

			if ( !SteamLobby.Instance.IsOwner() ) {
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