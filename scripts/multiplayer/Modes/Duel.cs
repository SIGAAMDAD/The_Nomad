using Godot;
using Multiplayer.Overlays;
using Renown;

namespace Multiplayer.Modes {
	public partial class Duel : Mode {
		private const int MaxRounds = 3; // NOTE: this may be adjustable in the future
		private Player ThisPlayer = null;
		private NetworkPlayer OtherPlayer = null;
		private Node2D Player1Spawn;
		private Node2D Player2Spawn;
		private DuelOverlay Overlay;
		private ScoreBoard ScoreBoard;

		private byte Player1Score = 0;
		private byte Player2Score = 0;
		private byte RoundIndex = 0;
		private byte[] Scores = new byte[ MaxRounds ];

		private NetworkSyncObject SyncObject = new NetworkSyncObject( 6 );

		private void OnNewRoundStart() {
			Announcer.Fight();

			ThisPlayer.BlockInput( false );

			ScoreBoard.Hide();
		}
		private void SendSpawnCommand( Node2D spawn ) {
			SyncObject.Write( (byte)SteamLobby.MessageType.ServerSync );
			SyncObject.Write( (byte)SteamLobby.Instance.GetMemberIndex( OtherPlayer.MultiplayerData.Id ) );
			SyncObject.Write( (byte)PlayerUpdateType.SetSpawn );
			SyncObject.Write( spawn.GlobalPosition );
		}
		public void OnRoundEnd() {
			if ( ThisPlayer == null || OtherPlayer == null ) {
				return;
			}

			GD.Print( "Beginning new dueling round..." );

			ThisPlayer?.BlockInput( true );

			Node2D otherSpawn = null;
			if ( (Entity)Player1Spawn.GetMeta( "Player" ) == ThisPlayer ) {
				Player1Spawn.SetMeta( "Player", OtherPlayer );
				Player2Spawn.SetMeta( "Player", ThisPlayer );
				otherSpawn = Player1Spawn;
				ThisPlayer.GlobalPosition = Player2Spawn.GlobalPosition;
			} else if ( (Entity)Player2Spawn.GetMeta( "Player" ) == ThisPlayer ) {
				Player2Spawn.SetMeta( "Player", OtherPlayer );
				Player1Spawn.SetMeta( "Player", ThisPlayer );
				otherSpawn = Player2Spawn;
				ThisPlayer.GlobalPosition = Player1Spawn.GlobalPosition;
			}

//			RoundIndex++;
//
//			if ( RoundIndex >= MaxRounds ) {
//				ScoreBoard.SetDuelData( Scores[ 0 ], Scores[ 1 ], Scores[ 2 ], ThisPlayer.MultiplayerData.Id, OtherPlayer.MultiplayerData.Id );
//				ServerCommandManager.SendCommand( ServerCommandType.EndGame );
//				//				EmitSignal( "ShowScoreboard" );
//				return;
//			}

			Overlay.SetPlayer1Score( Player1Score );
			Overlay.SetPlayer2Score( Player2Score );

			Overlay.BeginNewRound();

			if ( !SteamLobby.Instance.IsOwner() ) {
				return;
			}

			SendSpawnCommand( otherSpawn );
		}

		private void OnPlayerScore( Entity attacker, Entity target ) {
			//			attacker.Set( "MultiplayerKills", (int)attacker.Get( "MultiplayerKills" ) + 1 );
			//			target.Set( "MultiplayerDeaths", (int)target.Get( "MultiplayerDeaths" ) + 1 );

			//			SteamUserStats.StoreStats();

			if ( attacker == ThisPlayer ) {
				Player1Score++;
			} else if ( attacker == OtherPlayer ) {
				Player2Score++;
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

		public override void SpawnPlayer( Entity player ) {
			Node2D spawn = null;
			if ( SteamLobby.Instance.IsOwner() ) {
				if ( player is NetworkPlayer node && node != null ) {
					OtherPlayer = node;
					node.GlobalPosition = Player2Spawn.GlobalPosition;
					spawn = Player2Spawn;
				} else if ( player is Player owner && owner != null ) {
					ThisPlayer = owner;
					ThisPlayer.GlobalPosition = Player1Spawn.GlobalPosition;
					spawn = Player1Spawn;
				}
			} else {
				if ( player is NetworkPlayer node && node != null ) {
					OtherPlayer = node;
					node.GlobalPosition = Player1Spawn.GlobalPosition;
					spawn = Player1Spawn;
				} else if ( player is Player owner && owner != null ) {
					ThisPlayer = owner;
					ThisPlayer.GlobalPosition = Player1Spawn.GlobalPosition;
					spawn = Player1Spawn;
				}
			}
			player.Die += OnPlayerScore;
			GD.Print( "Set player " + player + " to spawn " + spawn );
			spawn.SetMeta( "Player", player );
		}

		private void SendPacket() {
			SyncObject.Write( (byte)SteamLobby.MessageType.GameData );
			SyncObject.Write( GetPath().GetHashCode() );
			SyncObject.Write( Player1Score );
			SyncObject.Write( Player2Score );
			SyncObject.Write( RoundIndex );

			for ( int i = 0; i < Scores.Length; i++ ) {
				SyncObject.Write( Scores[ i ] );
			}

			SyncObject.Sync();
		}
		private void ReceivePacket( System.IO.BinaryReader reader ) {
			SyncObject.BeginRead( reader );

			Player1Score = SyncObject.ReadByte();
			Player2Score = SyncObject.ReadByte();
			RoundIndex = SyncObject.ReadByte();

			for ( int i = 0; i < Scores.Length; i++ ) {
				Scores[ i ] = SyncObject.ReadByte();
			}

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
