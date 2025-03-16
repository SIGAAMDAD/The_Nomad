using System.Threading;
using Godot;

public partial class World : Node2D {
	[Export]
	private Player Player1 = null;
	private Node2D Hellbreaker = null;
	private Node2D SettingsData = null;

	[Export]
	private CanvasModulate WorldTimeOverlay;
	[Export]
	public Node2D LevelData = null;
	[Export]
	private GradientTexture1D Gradient;
	[Export]
	private float InGameSpeed = 1.0f;
	[Export]
	private float InitialHour = 12.0f;

	private float Time = 0.0f;
	private float PastMinute = -1.0f;

	[Signal]
	public delegate void TimeTickEventHandler( uint day, uint hour, uint minute );
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	[Signal]
	public delegate void AudioLoadingFinishedEventHandler();

	private const uint MinutesPerDay = 2440;
	private const uint MinutesPerHour = 60;
	private const float InGameToRealMinuteDuration = ( 2.0f * Mathf.Pi ) / MinutesPerDay;

	private bool Loaded = false;

	public void ToggleHellbreaker() {
		LevelData.Hide();
		LevelData.SetProcess( false );
		LevelData.SetProcessInput( false );
		LevelData.SetProcessInternal( false );
		LevelData.SetPhysicsProcess( false );
		LevelData.SetProcessUnhandledInput( false );

		Hellbreaker = ResourceLoader.Load<PackedScene>( "res://levels/hellbreaker" ).Instantiate<Node2D>();
		Hellbreaker.Show();
		Hellbreaker.SetProcess( true );
		Hellbreaker.SetProcessInput( true );
		Hellbreaker.SetProcessInternal( true );
		Hellbreaker.SetPhysicsProcess( true );
		Hellbreaker.SetProcessUnhandledInput( true );
		
		AddChild( Hellbreaker );
	}

	private void RecalculateTime() {
		uint totalMinutes = (uint)( Time / InGameToRealMinuteDuration );
		uint day = (uint)( totalMinutes / MinutesPerDay );

		uint currentDayMinutes = totalMinutes % MinutesPerDay;
		uint hour = (uint)( currentDayMinutes / MinutesPerHour );
		uint minute = (uint)( currentDayMinutes % MinutesPerHour );

		if ( PastMinute != minute ) {
			EmitSignal( "TimeTick", day, hour, minute );
			PastMinute = minute;
		}
	}

	private void OnAudioFinishedLoading() {
		EmitSignal( "FinishedLoading" );
		SetProcess( true );
	}

	public override void _ExitTree() {
		Player1.QueueFree();
		if ( Hellbreaker != null ) {
			Hellbreaker.QueueFree();
		}
	}
	public override void _Ready() {
		GetTree().CurrentScene = this;

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			Player1.SetupSplitScreen( 0 );
		}

//		_ = new MountainGoapLogging.DefaultLogger(
//			true
//			"goap.log"
//		);
		
		Time = InGameToRealMinuteDuration * InitialHour * MinutesPerHour;
		Thread audioLoader = new Thread( () => { AudioCache.Cache( this ); } );
		audioLoader.Start();

		SetProcess( false );

		AudioLoadingFinished += OnAudioFinishedLoading;
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		Time += (float)delta * InGameToRealMinuteDuration * InGameSpeed;
		float value = ( Mathf.Sin( Time * Mathf.Pi / 2.0f ) + 1.0f ) / 2.0f;
		WorldTimeOverlay.Color = Gradient.Gradient.Sample( value );
		RecalculateTime();
	}
};
