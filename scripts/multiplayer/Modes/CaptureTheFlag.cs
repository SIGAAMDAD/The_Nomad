using Godot;
using Multiplayer.Overlays;
using Renown;
using Steamworks;
using System.Collections.Generic;

namespace Multiplayer.Modes {
	public partial class CaptureTheFlag : Mode {
		[Export]
		private Team RedTeam;
		[Export]
		private Team BlueTeam;

		private List<Spawner> Spawns;

		private CaptureTheFlagOverlay Overlay;

		private void OnFlagReturned( Flag flag, Entity source ) {
		}
		private void OnFlagStolen( Flag flag, Entity source ) {
		}
		private void OnFlagCaptured( Flag flag, Entity source ) {
		}

		public override void OnPlayerJoined( Entity player ) {
			Team team = BlueTeam;
			if ( RedTeam.GetMemberCount() < team.GetMemberCount() ) {
				team = RedTeam;
			}
			team.AddPlayer( player );
		}
		public override void OnPlayerLeft( Entity player ) {
			// auto-balance teams
		}
		public override void SpawnPlayer( Entity player ) {
		}

		public override bool HasTeams() => true;
		public override GameMode GetMode() => GameMode.CaptureTheFlag;

		public override void _Ready() {
			base._Ready();

			if ( !SteamLobby.Instance.IsOwner() ) {
				return;
			}

			Godot.Collections.Array<Node> spawns = GetTree().GetNodesInGroup( "Spawns" );
			Spawns = new List<Spawner>( spawns.Count );
			for ( int i = 0; i < spawns.Count; i++ ) {
				Spawns.Add( spawns[ i ] as Spawner );
			}

			Flag RedFlag = GetNode<Flag>( "ModeData/RedFlag" );
			RedFlag.Returned += ( source ) => OnFlagReturned( RedFlag, source );
			RedFlag.Stolen += ( source ) => OnFlagReturned( RedFlag, source );
			RedFlag.Captured += ( source ) => OnFlagCaptured( RedFlag, source );

			Flag BlueFlag = GetNode<Flag>( "ModeData/BlueFlag" );
			BlueFlag.Returned += ( source ) => OnFlagReturned( BlueFlag, source );
			BlueFlag.Stolen += ( source ) => OnFlagReturned( BlueFlag, source );
			BlueFlag.Captured += ( source ) => OnFlagCaptured( BlueFlag, source );

			Overlay = GetNode<CaptureTheFlagOverlay>( "ModeData/Overlay" );
		}
	};
};