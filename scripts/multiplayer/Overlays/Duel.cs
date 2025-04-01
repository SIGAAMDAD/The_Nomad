using Godot;

namespace Multiplayer.Overlays {
	public partial class Duel : CanvasLayer {
		private Label MatchTimeLabel;

		private Label Player1Score;
		private Label Player2Score;

		private void OnDuelTimerTimeout() {

		}

		public override void _Ready() {
			base._Ready();

			MatchTimeLabel = GetNode<Label>( "MarginContainer/VBoxContainer/MatchTimeLabel" );
			MatchTimeLabel.Call( "SetMatchTime", 180.0f, Callable.From( OnDuelTimerTimeout ) );

			Player1Score = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/Player1ScoreLabel" );
			Player2Score = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/Player2ScoreLabel" );
		}
	};
};