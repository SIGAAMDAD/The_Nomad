using System.Runtime.CompilerServices;
using Godot;

namespace Multiplayer.Overlays {
	public partial class CaptureTheFlagOverlay : CanvasLayer {
		private MatchTimeLabel MatchTimeLabel;

		private Label TeamRedScore;
		private Label TeamBlueScore;

		private Countdown CountdownLabel;

		[Signal]
		public delegate void RoundStartEventHandler();
		[Signal]
		public delegate void RoundEndEventHandler();

		public void SetTeamRedScore( int nScore ) => TeamRedScore.Text = nScore.ToString();
		public void SetTeamBlueScore( int nScore ) => TeamBlueScore.Text = nScore.ToString();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void NewRound() {
			SetProcess( true );
			CountdownLabel.Visible = true;
			CountdownLabel.StartCountdown();
			EmitSignal( "RoundEnd" );
		}
		public void BeginNewRound() => NewRound();

		private void OnCaptureTheFlagTimerTimeout() {
			
		}
	};
};