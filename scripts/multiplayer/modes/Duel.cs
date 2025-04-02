using Godot;
using Multiplayer.Overlays;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class Duel : Mode {
		private int Player1Score = 0;
		private int Player2Score = 0;
		private int MaxRounds = 2; // NOTE: this may be adjustable in the future
		private int RoundIndex = 0;
		private CharacterBody2D Player1 = null;
		private CharacterBody2D Player2 = null;
		private Node2D Player1Spawn;
		private Node2D Player2Spawn;
		private DuelOverlay Overlay;

		private NetworkWriter SyncObject = new NetworkWriter( 12 );

		private void OnNewRoundStart() {
			Announcer.Fight();
			if ( Player1 is Player ) {
				Player1.Call( "BlockInput", false );
			} else {
				Player2.Call( "BlockInput", false );
			}
		}
		public void OnRoundEnd() {
			GD.Print( "Beginning new dueling round..." );

			if ( Player1 is Player ) {
				Player1.Call( "BlockInput", true );
				Player1.GlobalPosition = Player1Spawn.GlobalPosition;
			} else {
				Player2.Call( "BlockInput", true );
				Player2.GlobalPosition = Player2Spawn.GlobalPosition;
			}
			
			RoundIndex++;
			
			if ( RoundIndex >= MaxRounds ) {
				Overlay.Call( "ShowScoreboard" );
				return;
			}
			
			Overlay.SetPlayer1Score( Player1Score );
			Overlay.SetPlayer2Score( Player2Score );
			Overlay.BeginNewRound();

			if ( SteamLobby.Instance.IsOwner() ) {
				SyncObject.Write( Player1Score );
				SyncObject.Write( Player2Score );
				SyncObject.Write( RoundIndex );
				SyncObject.Sync();
			}
		}

		private void OnPlayerScore( CharacterBody2D attacker, CharacterBody2D target ) {
//			attacker.Set( "MultiplayerKills", (int)attacker.Get( "MultiplayerKills" ) + 1 );
//			target.Set( "MultiplayerDeaths", (int)target.Get( "MultiplayerDeaths" ) + 1 );

//			SteamUserStats.StoreStats();

			if ( attacker == Player1 ) {
				Player1Score++;
			} else if ( attacker == Player2 ) {
				Player2Score++;
			}
			if ( Player1 is Player ) {
				if ( Player2Score > Player1Score ) {
					Announcer.LostLead();
				} else if ( Player2Score == Player1Score ) {
					Announcer.TiedLead();
				} else if ( Player2Score < Player1Score ) {
					Announcer.TakenLead();
				}
			} else if ( Player2 is Player ) {
				if ( Player1Score > Player2Score ) {
					Announcer.LostLead();
				} else if ( Player1Score == Player2Score ) {
					Announcer.TiedLead();
				} else if ( Player1Score < Player2Score ) {
					Announcer.TakenLead();
				}
			}

			OnNewRoundStart();
		}

		public override void OnPlayerJoined( CharacterBody2D player ) {
			player.Connect( "Die", Callable.From<CharacterBody2D, CharacterBody2D>( OnPlayerScore ) );

			if ( Player1 == null ) {
				Player1 = player;
			} else {
				Player2 = player;
			}
		}
		public override void OnPlayerLeft( CharacterBody2D player ) {
		}
		public override void SpawnPlayer( CharacterBody2D player ) {
			if ( player == Player1 ) {
				player.GlobalPosition = Player1Spawn.GlobalPosition;
			} else if ( player == Player2 ) {
				player.GlobalPosition = Player2Spawn.GlobalPosition;
			}
		}

		public override void _Ready() {
			base._Ready();
			
			Overlay = GetNode<DuelOverlay>( "Overlay" );
			Overlay.SetProcessInternal( false );
			Overlay.Connect( "RoundEnd", Callable.From( OnRoundEnd ) );
			Overlay.Connect( "RoundStart", Callable.From( OnNewRoundStart ) );
			Overlay.Call( "BeginNewRound" );

			Player1Spawn = GetNode<Node2D>( "Player1Spawn" );
			Player1Spawn.SetProcess( false );
			Player1Spawn.SetProcessInternal( false );

			Player2Spawn = GetNode<Node2D>( "Player2Spawn" );
			Player2Spawn.SetProcess( false );
			Player2Spawn.SetProcessInternal( false );
		}
    };
};
