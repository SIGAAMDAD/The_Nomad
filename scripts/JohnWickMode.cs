using Godot;
using Renown;
using System;
using System.Diagnostics;

/*
public partial class JohnWickMode : LevelData {
	private Stopwatch Timer;

	private static JohnWickMode Mode;

	private static int TotalScore = 0;

	private Label BountyValueLabel;
	private Label KillCountLabel;
	private Label TimeLabel;

	private bool HasWeapon = false;
	private bool HasAmmo = false;
	private int Marker = 0;

	[Signal]
	public delegate void OneMinuteMarkerEventHandler();
	[Signal]
	public delegate void ThreeMinuteMarkerEventHandler();
	[Signal]
	public delegate void TenMinuteMarkerEventHandler();

	private void OnPlayerDie( Entity source, Entity target ) {
		Timer.Stop();

		FreeFlow.EndCombo();

		//
		// calculate total score
		//
		TotalScore += FreeFlow.GetHighestCombo() * 10;

		int milliseconds = Timer.Elapsed.Milliseconds;
		int seconds = Timer.Elapsed.Seconds;
		int minutes = Timer.Elapsed.Minutes;

		GetNode<CanvasLayer>( "Overlay" ).Hide();

		ThisPlayer.BlockInput( true );
		ChallengeModeScore ScoreOverlay = ResourceCache.GetScene( "res://scenes/menus/challenge_mode_score.tscn" ).Instantiate<ChallengeModeScore>();
		AddChild( ScoreOverlay );
		ScoreOverlay.SetScores(
			new ScoreData(
				"In Da Club",
				TotalScore, FreeFlow.GetHighestCombo(), minutes, seconds, milliseconds, FreeFlow.GetKillCounter(),
				1, ChallengeLevel.ScoreBonus.None
			)
		);
	}

	public static void AddKill() {
		//		Mode.KillCountLabel.Text = "KILL COUNT: " + KillCounter.ToString();
	}
	public static void IncreaseScore( int nAmount ) {
		System.Threading.Interlocked.Add( ref TotalScore, nAmount );
		Mode.BountyValueLabel.Text = "BOUNTY VALUE: " + TotalScore.ToString();
	}

	protected override void OnResourcesFinishedLoading() {
		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive );

		Console.PrintLine( "...Finished loading game" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );

		Timer = new Stopwatch();
		Timer.Start();

		SetProcess( true );
	}

	public static int GetTimeMinutes() => Mode.Timer.Elapsed.Minutes;

	public override void _EnterTree() {
		base._EnterTree();

		Mode = this;
	}

	public override void _Ready() {
		base._Ready();

		ResourceLoadThread = new System.Threading.Thread( () => { ResourceCache.Cache( this, null ); } );
		ResourceLoadThread.Start();

		ThisPlayer.Die += OnPlayerDie;

		KillCountLabel = GetNode<Label>( "Overlay/VBoxContainer/KillCountLabel" );
		BountyValueLabel = GetNode<Label>( "Overlay/VBoxContainer/BountyValueLabel" );
		TimeLabel = GetNode<Label>( "Overlay/VBoxContainer/TimeLabel" );

		AudioStreamPlayer Theme = GetNode<AudioStreamPlayer>( "Theme" );
		Theme.VolumeDb = SettingsData.GetMusicVolumeLinear();

		GetViewport().GetCamera2D().Zoom = new Vector2( 1.0f, 1.0f );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		switch ( Marker ) {
			case 0:
				if ( Timer.Elapsed.Minutes >= 1 ) {
					EmitSignalOneMinuteMarker();
					Marker++;
				}
				break;
			case 1:
				if ( Timer.Elapsed.Minutes >= 3 ) {
					EmitSignalThreeMinuteMarker();
					Marker++;
				}
				break;
			case 2:
				if ( Timer.Elapsed.Minutes >= 10 ) {
					EmitSignalTenMinuteMarker();
					Marker++;
				}
				break;
			default:
				break;
		}
		;

		TimeLabel.Text = string.Format( "{0}:{1}.{2}", Timer.Elapsed.Minutes, Timer.Elapsed.Seconds, Timer.Elapsed.Milliseconds );
	}
};
*/