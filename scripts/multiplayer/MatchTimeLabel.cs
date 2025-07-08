using Godot;

public partial class MatchTimeLabel : Label {
	private Timer Timer;

	public void SetMatchTime( double time, Callable callback ) {
		Timer.WaitTime = time;
		Timer.OneShot = true;
		if ( !Timer.IsConnected( "timeout", callback ) ) {
			Timer.Connect( "timeout", callback );
		}
	}
	public void Start() {
		Timer.Start();
		SetProcess( true );
	}

	public override void _Ready() {
		base._Ready();

		Timer = GetNode<Timer>( "Timer" );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		Text = Timer.TimeLeft.ToString( "F2" );
	}
};
