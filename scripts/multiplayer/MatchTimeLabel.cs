using Godot;

public partial class MatchTimeLabel : Label {
	private Timer Timer;

	public void SetMatchTime( double time, Callable callback ) {
		Timer.WaitTime = time;
		Timer.OneShot = true;
		if ( !Timer.IsConnected( "timeout", callback ) ) {
			Timer.Connect( "timeout", callback );
		}
		Timer.Start();
	}

	public override void _Ready() {
		base._Ready();

		Timer = GetNode<Timer>( "Timer" );
		Timer.SetProcess( false );
		Timer.SetProcessInternal( false );

		SetProcessInternal( false );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		Text = Timer.TimeLeft.ToString( "F2" );
	}
};
