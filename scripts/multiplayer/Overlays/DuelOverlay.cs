/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using System.Runtime.CompilerServices;
using Godot;
using Steam;

namespace Multiplayer.Overlays {
	/*
	===================================================================================
	
	DuelOverlay
	
	===================================================================================
	*/
	
	public partial class DuelOverlay : CanvasLayer {
		public MatchTimeLabel MatchTimeLabel;

		private Label OurScore;
		private Label TheirScore;

		private Countdown CountdownLabel;

		[Signal]
		public delegate void RoundStartEventHandler();
		[Signal]
		public delegate void RoundEndEventHandler();

		public void SetPlayer1Score( int score ) {
			if ( SteamLobby.Instance.IsHost ) {
				OurScore.Text = score.ToString();
			} else {
				TheirScore.Text = score.ToString();
			}
		}
		public void SetPlayer2Score( int score ) {
			if ( SteamLobby.Instance.IsHost ) {
				TheirScore.Text = score.ToString();
			} else {
				OurScore.Text = score.ToString();
			}
		}
		public void SetRemainingTime( float time ) => CountdownLabel.SetTimeLeft( time );
		public float GetRemainingTime() => CountdownLabel.GetTimeLeft();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void NewRound() {
			SetProcess( true );
			CountdownLabel.Visible = true;
			CountdownLabel.StartCountdown();
			EmitSignalRoundEnd();
		}
		public void BeginNewRound() => NewRound();
		public void RestartRound() => NewRound();

		private void OnDuelTimerTimeout() {
			RestartRound();
		}
		private void OnCountdownTimerTimeout() {
			SetProcess( false );

			CountdownLabel.Visible = false;
			EmitSignalRoundStart();

			MatchTimeLabel.Start();
		}

		public override void _Ready() {
			base._Ready();

			MatchTimeLabel = GetNode<MatchTimeLabel>( "MarginContainer/VBoxContainer/MatchTimeLabel" );
			MatchTimeLabel.SetMatchTime( 60.0f, Callable.From( OnDuelTimerTimeout ) );

			CountdownLabel = GetNode<Countdown>( "MarginContainer/CountdownLabel" );
			CountdownLabel.Connect( Countdown.SignalName.CountdownTimeout, Callable.From( OnCountdownTimerTimeout ) );

			OurScore = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/Player1ScoreLabel" );
			TheirScore = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/Player2ScoreLabel" );
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			CountdownLabel.Update();
		}
	};
};