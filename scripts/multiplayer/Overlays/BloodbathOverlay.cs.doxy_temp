using Godot;

namespace Multiplayer.Overlays {
	public partial class BloodbathOverlay : CanvasLayer {
		private MatchTimeLabel MatchTimeLabel;

		private Label PointLabel;
		private Label RankingLabel;

		private Countdown CountdownLabel;

		[Signal]
		public delegate void GameStartEventHandler();
		[Signal]
		public delegate void GameEndEventHandler();

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

			PointLabel = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreOverlay/PointLabel" );
			RankingLabel = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreOverlay/RankingLabel" );
		}
	};
};
