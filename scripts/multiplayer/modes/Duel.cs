using Godot;
using Multiplayer.Overlays;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class Duel : Mode {
		private int Player1Score = 0;
		private int Player2Score = 0;
		private int MaxRounds = 2; // NOTE: this may be adjustable in the future
		private int RoundIndex = 0;
		private Player ThisPlayer = null;
		private NetworkPlayer OtherPlayer = null;
		private Node2D Player1Spawn;
		private Node2D Player2Spawn;
		private DuelOverlay Overlay;
		private ScoreBoard ScoreBoard;

		private int[] Score = new int[ 3 ];

		private NetworkWriter SyncObject = new NetworkWriter( 20 );

		private void OnNewRoundStart() {
			Announcer.Fight();
			ThisPlayer.BlockInput( false );

			ScoreBoard.Hide();
		}
		public void OnRoundEnd() {
			GD.Print( "Beginning new dueling round..." );

			if ( ThisPlayer == null || OtherPlayer == null ) {
				return;
			}

			ThisPlayer?.BlockInput( true );

			if ( RoundIndex >= MaxRounds ) {
				ScoreBoard.SetDuelData( Score[0], Score[1], Score[2], ThisPlayer.MultiplayerData.Id, OtherPlayer.MultiplayerData.Id );
//				EmitSignal( "ShowScoreboard" );
				return;
			}

			if ( !SteamLobby.Instance.IsOwner() ) {
				return;
			}
			RoundIndex++;
			
			Overlay.SetPlayer1Score( Player1Score );
			Overlay.SetPlayer2Score( Player2Score );
			Overlay.BeginNewRound();

			SendPacket();
		}

		private void OnPlayerScore( Renown.Entity attacker, Renown.Entity target ) {
//			attacker.Set( "MultiplayerKills", (int)attacker.Get( "MultiplayerKills" ) + 1 );
//			target.Set( "MultiplayerDeaths", (int)target.Get( "MultiplayerDeaths" ) + 1 );

//			SteamUserStats.StoreStats();

			if ( attacker == ThisPlayer ) {
				Player1Score++;
				Score[ RoundIndex ] = 0;
			} else if ( attacker == OtherPlayer ) {
				Player2Score++;
				Score[ RoundIndex ] = 1;
			}
			if ( Player2Score > Player1Score ) {
				Announcer.LostLead();
			} else if ( Player2Score == Player1Score ) {
				Announcer.TiedLead();
			} else if ( Player2Score < Player1Score ) {
				Announcer.TakenLead();
			}

			OnNewRoundStart();
		}

		public override void SpawnPlayer( Renown.Entity player ) {
			if ( SteamLobby.Instance.IsOwner() ) {
				if ( player is NetworkPlayer node && node != null ) {
					node.GlobalPosition = Player2Spawn.GlobalPosition;
					node.Die += OnPlayerScore;
				} else if ( player is Player owner && owner != null ) {
					ThisPlayer = owner;
					ThisPlayer.GlobalPosition = Player1Spawn.GlobalPosition;
					ThisPlayer.Die += OnPlayerScore;
				}
			} else {
				if ( player is NetworkPlayer node && node != null ) {
					node.GlobalPosition = Player1Spawn.GlobalPosition;
					node.Die += OnPlayerScore;
				} else if ( player is Player owner && owner != null ) {
					ThisPlayer = owner;
					ThisPlayer.GlobalPosition = Player2Spawn.GlobalPosition;
					ThisPlayer.Die += OnPlayerScore;
				}
			}
		}

		private void SendPacket() {
			SyncObject.Write( Player1Score );
			SyncObject.Write( Player2Score );
			SyncObject.Write( RoundIndex );
			SyncObject.Sync();
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			Player1Score = reader.ReadInt32();
			Player2Score = reader.ReadInt32();
			RoundIndex = reader.ReadInt32();

			Overlay.SetPlayer1Score( Player1Score );
			Overlay.SetPlayer2Score( Player2Score );
		}

		public override void _Ready() {
			base._Ready();
			
			Overlay = GetNode<DuelOverlay>( "Overlay" );
			Overlay.Connect( "RoundEnd", Callable.From( OnRoundEnd ) );
			Overlay.Connect( "RoundStart", Callable.From( OnNewRoundStart ) );

			Player1Spawn = GetNode<Node2D>( "Player1Spawn" );
			Player1Spawn.SetProcess( false );
			Player1Spawn.SetProcessInternal( false );

			Player2Spawn = GetNode<Node2D>( "Player2Spawn" );
			Player2Spawn.SetProcess( false );
			Player2Spawn.SetProcessInternal( false );

			ScoreBoard = GetNode<ScoreBoard>( "Scoreboard" );

			if ( SteamLobby.Instance.IsOwner() ) {
				Overlay.BeginNewRound();
			} else {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			}
		}
    };
};
