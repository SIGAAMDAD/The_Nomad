using Godot;
using Multiplayer.Overlays;
using Renown;
using Steamworks;
using System.Collections.Generic;

namespace Multiplayer.Modes {
	public partial class Duel : Mode {
		public static readonly int MinPlayers = 1;
		public static readonly int MaxPlayers = 2;

		private int MaxRounds = 3; // NOTE: this may be adjustable in the future
		private Player ThisPlayer = null;
		private NetworkPlayer OtherPlayer = null;
		private Node2D Player1Spawn;
		private Node2D Player2Spawn;
		private DuelOverlay Overlay;
		private ScoreBoard ScoreBoard;

		private byte Player1Score = 0;
		private byte Player2Score = 0;
		private byte RoundIndex = 0;
		private byte[] Scores = new byte[ 3 ];

		private System.Threading.Thread WaitThread;

		private NetworkSyncObject SyncObject = new NetworkSyncObject( 16 );

		public readonly static Dictionary<string, object> DefaultOptions = new Dictionary<string, object> {
			{ "MaxRounds", 3 },
			{ "RoundTime", 60.0f }
		};

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
			SyncObject.Sync(); 
		}
		public void OnRoundEnd() {
			if ( ThisPlayer == null || OtherPlayer == null ) {
				return;
			}

			ThisPlayer?.BlockInput( true );

			Node2D otherSpawn;
			if ( Player1Spawn.GetMeta( "Player" ).AsGodotObject() is Player player && player != null ) {
				Player1Spawn.SetMeta( "Player", OtherPlayer );
				Player2Spawn.SetMeta( "Player", ThisPlayer );
				otherSpawn = Player1Spawn;
				ThisPlayer.GlobalPosition = Player2Spawn.GlobalPosition;
			} else {
				Player1Spawn.SetMeta( "Player", ThisPlayer );
				Player2Spawn.SetMeta( "Player", OtherPlayer );
				otherSpawn = Player2Spawn;
				ThisPlayer.GlobalPosition = Player1Spawn.GlobalPosition;
			}

			ThisPlayer.MultiplayerReset();
			OtherPlayer.MultiplayerReset();

			Overlay.SetPlayer1Score( Player1Score );
			Overlay.SetPlayer2Score( Player2Score );

			if ( RoundIndex++ >= MaxRounds ) {
				ScoreBoard.SetDuelData( Scores[ 0 ], Scores[ 1 ], Scores[ 2 ], ThisPlayer.MultiplayerData.Id, OtherPlayer.MultiplayerData.Id );
				EmitSignalShowScoreboard();
				return;
			}

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
				Scores[ RoundIndex ] = 0;
			} else if ( attacker == OtherPlayer ) {
				Player2Score++;
				Scores[ RoundIndex ] = 1;
			}

			if ( Player2Score > Player1Score ) {
				Announcer.LostLead();
			} else if ( Player2Score == Player1Score ) {
				Announcer.TiedLead();
			} else if ( Player2Score < Player1Score ) {
				Announcer.TakenLead();
			}

			SendPacket();

			Overlay.BeginNewRound();
		}

		private Node2D SetPlayerSpawn( Entity player ) {
			Node2D spawn = null;
			if ( SteamLobby.Instance.IsOwner() ) {
				if ( player is NetworkPlayer node && node != null ) {
					OtherPlayer = node;
					node.GlobalPosition = Player2Spawn.GlobalPosition;
					spawn = Player2Spawn;

					// begin the match only when the other player joins
					WaitThread = new System.Threading.Thread( () => {
						while ( !SteamLobby.AllPlayersReady() ) {
							Console.PrintLine( "Synchronizing players..." );
							System.Threading.Thread.Sleep( 50 );
						}
						Console.PrintLine( "Players synched." );
						Overlay.CallDeferred( DuelOverlay.MethodName.BeginNewRound );
						CallDeferred( MethodName.JoinWaitThread );
					} );
					WaitThread.Start();
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

					// begin the match only when the other player joins
					WaitThread = new System.Threading.Thread( () => {
						while ( !SteamLobby.AllPlayersReady() ) {
							Console.PrintLine( "Synchronizing players..." );
							System.Threading.Thread.Sleep( 50 );
						}
						Console.PrintLine( "Players synched." );
						Overlay.CallDeferred( DuelOverlay.MethodName.BeginNewRound );
						CallDeferred( MethodName.JoinWaitThread );
					} );
					WaitThread.Start();
				} else if ( player is Player owner && owner != null ) {
					ThisPlayer = owner;
					ThisPlayer.GlobalPosition = Player2Spawn.GlobalPosition;
					spawn = Player2Spawn;
				}
			}
			spawn.SetMeta( "Player", player );
			return spawn;
		}

		private void JoinWaitThread() => WaitThread.Join();

		public override void SpawnPlayer( Entity player ) {
			SetPlayerSpawn( player );
			player.Die += OnPlayerScore;
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

		public override void OnPlayerJoined( Entity player ) {
		}
		public override void OnPlayerLeft( Entity player ) {
		}
		public override bool HasTeams() => false;
		public override GameMode GetMode() => GameMode.Duel;

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
			ScoreBoard.Connect( ScoreBoard.SignalName.LeaveGame, Callable.From( EmitSignalEndGame ) );

//			MaxRounds = (int)Options[ "MaxRounds" ];

			if ( !SteamLobby.Instance.IsOwner() ) {
				SteamLobby.Instance.AddNetworkNode( GetPath(), new SteamLobby.NetworkNode( this, null, ReceivePacket ) );
			}
		}
    };
};
