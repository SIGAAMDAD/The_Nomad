using Godot;

public partial class MatchTimeLabel : Label {
	private Timer Timer;

	public void SetMatchTime( double time, Callable callback ) {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return;
		}
		Timer.WaitTime = time;
		Timer.OneShot = true;
		if ( !Timer.IsConnected( "timeout", callback ) ) {
			Timer.Connect( "timeout", callback );
		}
	}
	public void Start() => Timer.Start();

	public override void _Ready() {
		base._Ready();

		Timer = GetNode<Timer>( "Timer" );
		SetProcess( true );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		Text = Timer.TimeLeft.ToString( "F2" );
	}
};
