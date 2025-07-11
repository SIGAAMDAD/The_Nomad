using Godot;

public partial class Countdown : Label {
	private Timer Timer;

	[Signal]
	public delegate void CountdownTimeoutEventHandler();

	public void Update() => Text = Timer.TimeLeft.ToString( "F2" );
	public void StartCountdown() => Timer.Start();
	public void SetTimeLeft( float time ) => Text = time.ToString( "F2" );
	public float GetTimeLeft() => (float)Timer.TimeLeft;

	public override void _Ready() {
		base._Ready();

		Timer = GetNode<Timer>( "Timer" );
		Timer.Connect( Timer.SignalName.Timeout, Callable.From( EmitSignalCountdownTimeout ) );
	}
};