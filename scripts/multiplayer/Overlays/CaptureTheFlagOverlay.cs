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