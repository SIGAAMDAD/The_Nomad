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

using Godot;

namespace Multiplayer.Overlays {
	public partial class CaptureTheFlagOverlay : CanvasLayer {
		public MatchTimeLabel MatchTimeLabel;

		private Label OurTeamScore;
		private Label TheirTeamScore;

		private Countdown CountdownLabel;

		[Signal]
		public delegate void GameStartEventHandler();
		[Signal]
		public delegate void GameEndEventHandler();

		public void SetTeamRedScore( int nScore ) {
			if ( LevelData.Instance.ThisPlayer.GetMeta( "Team" ).AsGodotObject().Get( "TeamIndex" ).AsInt32() == 0 ) {
				OurTeamScore.Text = nScore.ToString();
			} else {
				TheirTeamScore.Text = nScore.ToString();
			}
		}
		public void SetTeamBlueScore( int nScore ) {
			if ( LevelData.Instance.ThisPlayer.GetMeta( "Team" ).AsGodotObject().Get( "TeamIndex" ).AsInt32() == 1 ) {
				OurTeamScore.Text = nScore.ToString();
			} else {
				TheirTeamScore.Text = nScore.ToString();
			}
		}

		public void BeginGame() {
			SetProcess( true );
			CountdownLabel.Visible = true;
			CountdownLabel.StartCountdown();
		}
		private void OnCountdownTimerTimeout() {
			SetProcess( false );

			CountdownLabel.Visible = false;
			EmitSignalGameStart();

			MatchTimeLabel.Start();
		}

		public override void _Ready() {
			base._Ready();

			MatchTimeLabel = GetNode<MatchTimeLabel>( "MarginContainer/VBoxContainer/MatchTimeLabel" );
			MatchTimeLabel.SetMatchTime( 180.0f, Callable.From( EmitSignalGameEnd ) );

			CountdownLabel = GetNode<Countdown>( "MarginContainer/CountdownLabel" );
			CountdownLabel.Connect( Countdown.SignalName.CountdownTimeout, Callable.From( OnCountdownTimerTimeout ) );

			OurTeamScore = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/OurTeamScoreLabel" );
			TheirTeamScore = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/TheirTeamScoreLabel" );
		}
		public override void _Process( double delta ) {
			base._Process( delta );

			CountdownLabel.Update();
		}
	};
};