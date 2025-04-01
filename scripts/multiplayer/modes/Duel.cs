using Godot;
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
		private CanvasLayer Overlay;

		private void OnNewRoundStart() {
			if ( (ulong)Player1.Get( "MultiplayerId" ) == (ulong)SteamManager.GetSteamID() ) {
				Player1.Call( "BlockInput", false );
			} else {
				Player2.Call( "BlockInput", false );
			}
		}
		public void NewRound() {
			if ( (ulong)Player1.Get( "MultiplayerId" ) == (ulong)SteamManager.GetSteamID() ) {
				Player1.Call( "BlockInput", true );
			} else {
				Player2.Call( "BlockInput", true );
			}
			
			RoundIndex++;
			
			if ( RoundIndex >= MaxRounds ) {
				Overlay.Call( "ShowScoreboard" );
				return;
			}
			
			Overlay.Call( "SetPlayer1Score", Player1Score );
			Overlay.Call( "SetPlayer2Score", Player2Score );
			Overlay.Call( "BeginNewRound" );
		}

		private void OnPlayerScore( CharacterBody2D attacker, CharacterBody2D target ) {
			attacker.Set( "MultiplayerKills", (int)attacker.Get( "MultiplayerKills" ) + 1 ) );
			target.Set( "MultiplayerDeaths", (int)target.Get( "MultiplayerDeaths" ) + 1 );

			SteamUserStats.StoreStats();

			if ( attacker == Player1 ) {
				Player1Score++;
			} else if ( attacker == Player2 ) {
				Player2Score++;
			}
			NewRound();
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
			
			Overlay = GetNode<CanvasLayer>( "Overlay" );
			Overlay.Connect( "RoundStart", Callable.From( OnNewRoundStart ) );
			
			Player1Spawn = GetNode<Node2D>( "Player1Spawn" );
			Player2Spawn = GetNode<Node2D>( "Player2Spawn" );
		}
    };
};
