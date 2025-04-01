using Godot;
using Steamworks;

namespace Multiplayer.Modes {
	public partial class Duel : Mode {
		private int Player1Score = 0;
		private int Player2Score = 0;
		private int MaxRounds = 0;
		private int RoundIndex = 0;
		private CSteamID Player1 = CSteamID.Nil;
		private CSteamID Player2 = CSteamID.Nil;
		private Node2D Player1Spawn;
		private Node2D Player2Spawn;

		public void NewRound() {
			RoundIndex++;
		}

		private void OnPlayerScore( CharacterBody2D attacker, CharacterBody2D target ) {
			CSteamID attackerId = (CSteamID)(ulong)attacker.Get( "MultiplayerId" );

			if ( attackerId == Player1 ) {
				Player1Score++;
			} else if ( attackerId == Player2 ) {
				Player2Score++;
			}
		}

		public override void OnPlayerJoined( CharacterBody2D player ) {
			player.Connect( "Die", Callable.From<CharacterBody2D, CharacterBody2D>( OnPlayerScore ) );

			if ( Player1 == CSteamID.Nil ) {
				Player1 = (CSteamID)(ulong)player.Get( "MultiplayerId" );
			} else {
				Player2 = (CSteamID)(ulong)player.Get( "MultiplayerId" );
			}
		}
		public override void OnPlayerLeft( CharacterBody2D player ) {
		}
		public override void SpawnPlayer( CharacterBody2D player ) {
			if ( (ulong)player.Get( "MultiplayerId" ) == (ulong)Player1 ) {
				player.GlobalPosition = Player1Spawn.GlobalPosition;
				Console.PrintLine( "Spawning player at " + Player1Spawn.GlobalPosition );
			} else if ( (ulong)player.Get( "MultiplayerId" ) == (ulong)Player2 ) {
				player.GlobalPosition = Player2Spawn.GlobalPosition;
				Console.PrintLine( "Spawning player at " + Player2Spawn.GlobalPosition );
			}
		}

		public override void _Ready() {
			base._Ready();

			Player1Spawn = GetNode<Node2D>( "Player1Spawn" );
			Player2Spawn = GetNode<Node2D>( "Player2Spawn" );
		}
    };
};