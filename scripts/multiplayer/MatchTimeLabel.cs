using Godot;

public partial class MatchTimeLabel : Label {
	private Timer Timer;

	public void SetMatchTime( double time, Callable callback ) {
		Timer.WaitTime = time;
		Timer.OneShot = true;
		Timer.Connect( "timeout", callback );
	}

	public override void _Ready() {
		base._Ready();

		Timer = GetNode<Timer>( "Timer" );
	}
	public override void _Process( double delta ) {
		if ( ( Engine.GetProcessFrames() % 60 ) != 0 ) {
			return;
		}

		base._Process( delta );

		Text = Timer.TimeLeft.ToString();
	}
};
