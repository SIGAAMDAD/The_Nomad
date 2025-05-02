using System.Runtime.CompilerServices;
using Godot;

namespace Multiplayer.Overlays {
	public partial class DuelOverlay : CanvasLayer {
		private MatchTimeLabel MatchTimeLabel;

		private Label Player1Score;
		private Label Player2Score;

		private Countdown CountdownLabel;

		[Signal]
		public delegate void RoundStartEventHandler();
		[Signal]
		public delegate void RoundEndEventHandler();

		public void SetPlayer1Score( int nScore ) => Player1Score.Text = nScore.ToString();
		public void SetPlayer2Score( int nScore ) => Player2Score.Text = nScore.ToString();
		public void SetRemainingTime( float time ) => CountdownLabel.SetTimeLeft( time );
		public float GetRemainingTime() => CountdownLabel.GetTimeLeft();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void NewRound() {
			SetProcess( true );
			CountdownLabel.Visible = true;
			CountdownLabel.StartCountdown();
			EmitSignal( "RoundEnd" );
		}
		public void BeginNewRound() => NewRound();
		public void RestartRound() => NewRound();

		private void OnDuelTimerTimeout() {
			RestartRound();
		}
		private void OnCountdownTimerTimeout() {
			SetProcess( false );

			CountdownLabel.Visible = false;
			EmitSignal( "RoundStart" );

			MatchTimeLabel.SetMatchTime( 60.0f, Callable.From( OnDuelTimerTimeout ) );
		}

		public override void _Ready() {
			base._Ready();

			MatchTimeLabel = GetNode<MatchTimeLabel>( "MarginContainer/VBoxContainer/MatchTimeLabel" );
			MatchTimeLabel.SetProcessInternal( false );
			MatchTimeLabel.SetMatchTime( 60.0f, Callable.From( OnDuelTimerTimeout ) );

			CountdownLabel = GetNode<Countdown>( "MarginContainer/CountdownLabel" );
			CountdownLabel.SetProcessInternal( false );
			CountdownLabel.Connect( "CountdownTimeout", Callable.From( OnCountdownTimerTimeout ) );

			Player1Score = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/Player1ScoreLabel" );
			Player1Score.SetProcess( false );
			Player1Score.SetProcessInternal( false );

			Player2Score = GetNode<Label>( "MarginContainer/VBoxContainer/ScoreContainer/Player2ScoreLabel" );
			Player2Score.SetProcess( false );
			Player2Score.SetProcessInternal( false );

			SetProcess( false );
			SetProcessInternal( false );
		}
		public override void _Process( double delta ) {
			base._Process( delta );

//			CountdownLabel.Update();
		}
	};
};