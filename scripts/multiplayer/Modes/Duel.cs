using Godot;
using Multiplayer.Overlays;
using Renown;

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

		private NetworkSyncObject SyncObject = new NetworkSyncObject( 3 );

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
			
			if ( (Entity)Player1Spawn.GetMeta( "Player" ) == ThisPlayer ) {
				Player1Spawn.SetMeta( "Player", OtherPlayer );
				Player2Spawn.SetMeta( "Player", ThisPlayer );
				ThisPlayer.GlobalPosition = Player2Spawn.GlobalPosition;
			} else if ( (Entity)Player2Spawn.GetMeta( "Player" ) == ThisPlayer ) {
				Player2Spawn.SetMeta( "Player", OtherPlayer );
				Player1Spawn.SetMeta( "Player", ThisPlayer );
				ThisPlayer.GlobalPosition = Player1Spawn.GlobalPosition;
			}

			RoundIndex++;

			if ( RoundIndex >= MaxRounds ) {
				ScoreBoard.SetDuelData( Score[ 0 ], Score[ 1 ], Score[ 2 ], ThisPlayer.MultiplayerData.Id, OtherPlayer.MultiplayerData.Id );
				ServerCommandManager.SendCommand( ServerCommandType.EndGame );
				//				EmitSignal( "ShowScoreboard" );
				return;
			}

			if ( !SteamLobby.Instance.IsOwner() ) {
				return;
			}
			
			Overlay.SetPlayer1Score( Player1Score );
			Overlay.SetPlayer2Score( Player2Score );
			Overlay.BeginNewRound();
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

			SendPacket();

			OnNewRoundStart();
		}

		public override void SpawnPlayer( Renown.Entity player ) {
			Node2D spawn = null;
			if ( SteamLobby.Instance.IsOwner() ) {
				if ( player is NetworkPlayer node && node != null ) {
					OtherPlayer = node;
					node.GlobalPosition = Player1Spawn.GlobalPosition;
					spawn = Player1Spawn;
				} else if ( player is Player owner && owner != null ) {
					ThisPlayer = owner;
					ThisPlayer.GlobalPosition = Player1Spawn.GlobalPosition;
					spawn = Player1Spawn;
				}
			} else {
				if ( player is NetworkPlayer node && node != null ) {
					OtherPlayer = node;
					node.GlobalPosition = Player2Spawn.GlobalPosition;
					spawn = Player2Spawn;
				} else if ( player is Player owner && owner != null ) {
					ThisPlayer = owner;
					ThisPlayer.GlobalPosition = Player2Spawn.GlobalPosition;
					spawn = Player2Spawn;
				}
			}
			player.Die += OnPlayerScore;
			spawn.SetMeta( "Player", player );
		}

		private void SendPacket() {
			SyncObject.Write( (sbyte)Player1Score );
			SyncObject.Write( (sbyte)Player2Score );
			SyncObject.Write( (sbyte)RoundIndex );
			SyncObject.Sync();
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			SyncObject.BeginRead( reader );

			Player1Score = SyncObject.ReadSByte();
			Player2Score = SyncObject.ReadSByte();
			RoundIndex = SyncObject.ReadSByte();

			Overlay.SetPlayer1Score( Player1Score );
			Overlay.SetPlayer2Score( Player2Score );

			if ( Player2Score > Player1Score ) {
				Announcer.LostLead();
			} else if ( Player2Score == Player1Score ) {
				Announcer.TiedLead();
			} else if ( Player2Score < Player1Score ) {
				Announcer.TakenLead();
			}
		}

		public override void _Ready() {
			base._Ready();
			
			Overlay = GetNode<DuelOverlay>( "Overlay" );
			Overlay.Connect( "RoundEnd", Callable.From( OnRoundEnd ) );
			Overlay.Connect( "RoundStart", Callable.From( OnNewRoundStart ) );

			Player1Spawn = GetNode<Node2D>( "Player1Spawn" );
			Player1Spawn.SetMeta( "Player", Godot.Variant.From( 0 ) );

			Player2Spawn = GetNode<Node2D>( "Player2Spawn" );
			Player2Spawn.SetMeta( "Player", Godot.Variant.From( 0 ) );

			ScoreBoard = GetNode<ScoreBoard>( "Scoreboard" );

			if ( SteamLobby.Instance.IsOwner() ) {
				Overlay.BeginNewRound();
			} else {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			}
		}
    };
};
