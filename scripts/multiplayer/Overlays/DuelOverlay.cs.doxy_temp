using System.Runtime.CompilerServices;
using Godot;

namespace Multiplayer.Overlays {
	public partial class DuelOverlay : CanvasLayer {
		public MatchTimeLabel MatchTimeLabel;

		private Label OurScore;
		private Label TheirScore;

		private Countdown CountdownLabel;

		[Signal]
		public delegate void RoundStartEventHandler();
		[Signal]
		public delegate void RoundEndEventHandler();

		public void SetPlayer1Score( int nScore ) {
			if ( SteamLobby.Instance.IsOwner() ) {
				OurScore.Text = nScore.ToString();
			} else {
				TheirScore.Text = nScore.ToString();
			}
		}
		public void SetPlayer2Score( int nScore ) {
			if ( SteamLobby.Instance.IsOwner() ) {
				TheirScore.Text = nScore.ToString();
			} else {
				OurScore.Text = nScore.ToString();
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