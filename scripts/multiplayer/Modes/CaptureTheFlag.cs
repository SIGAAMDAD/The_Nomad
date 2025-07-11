using Godot;
using Multiplayer.Overlays;
using Multiplayer.Objectives;
using Renown;
using System.Collections.Generic;

namespace Multiplayer.Modes {
	public partial class CaptureTheFlag : Mode {
		private enum FlagAction : int {
			Returned,
			Captured,
			Stoen,

			Count
		};

		[Export]
		private Team RedTeam;
		[Export]
		private Team BlueTeam;

		private List<Spawner> Spawns;

		private CaptureTheFlagOverlay Overlay;

		private byte Team1Score = 0;
		private byte Team2Score = 0;
		private NetworkSyncObject SyncObject = new NetworkSyncObject( 16 );

		private System.Threading.Thread WaitThread;

		private List<int, System.Action> ;

		private void OnFlagReturned( Flag flag, Entity source ) {
		}
		private void OnFlagStolen( Flag flag, Entity source ) {
		}
		private void OnFlagCaptured( Flag flag, Entity source ) {
		}

		private void SendPacket() {
			SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );

			SyncObject.Write( Team1Score );
			SyncObject.Write( Team2Score );
		}
		private void ReceivePacket( System.IO.BinaryReader packet ) {
			SyncObject.BeginRead( packet );

			Team1Score = SyncObject.ReadByte();
			Team2Score = SyncObject.ReadByte();
		}

		public override void SpawnPlayer( Entity player ) {
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
		public override bool HasTeams() => true;
		public override GameMode GetMode() => GameMode.CaptureTheFlag;

		private void JoinWaitThread() => WaitThread.Join();

		public override void _Ready() {
			base._Ready();

			Overlay = GetNode<CaptureTheFlagOverlay>( "ModeData/Overlay" );

			WaitThread = new System.Threading.Thread( () => {
				LevelData.Instance.ThisPlayer.CallDeferred( Player.MethodName.BlockInput, true );
				while ( !SteamLobby.AllPlayersReady() ) {
					System.Threading.Thread.Sleep( 50 );
				}
				LevelData.Instance.ThisPlayer.CallDeferred( Player.MethodName.BlockInput, false );
				Overlay.CallDeferred( CaptureTheFlagOverlay.MethodName.BeginGame );
				CallDeferred( MethodName.JoinWaitThread );
			} );
			WaitThread.Start();

			if ( !SteamLobby.Instance.IsOwner() ) {
				return;
			}

			Godot.Collections.Array<Node> spawns = GetTree().GetNodesInGroup( "Spawns" );
			Spawns = new List<Spawner>( spawns.Count );
			for ( int i = 0; i < spawns.Count; i++ ) {
				Spawns.Add( spawns[ i ] as Spawner );
			}

			Flag RedFlag = GetNode<Flag>( "ModeData/Flags/RedFlag" );
			RedFlag.Returned += ( source ) => OnFlagReturned( RedFlag, source );
			RedFlag.Stolen += ( source ) => OnFlagReturned( RedFlag, source );
			RedFlag.Captured += ( source ) => OnFlagCaptured( RedFlag, source );

			Flag BlueFlag = GetNode<Flag>( "ModeData/Flags/BlueFlag" );
			BlueFlag.Returned += ( source ) => OnFlagReturned( BlueFlag, source );
			BlueFlag.Stolen += ( source ) => OnFlagReturned( BlueFlag, source );
			BlueFlag.Captured += ( source ) => OnFlagCaptured( BlueFlag, source );

			if ( SteamLobby.Instance.IsOwner() ) {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			}
		}
	};
};