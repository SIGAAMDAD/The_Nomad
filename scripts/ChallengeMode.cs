using System;
using System.Diagnostics;
using System.Threading;
using ChallengeMode;
using Godot;

// TODO: allow multiplayer with challenge modes?

public partial class ChallengeLevel : Node2D {
	private enum ScoreBonus {
		None		= 0x0000,
		NoDamage	= 0x0001,
		NoDeaths	= 0x0002,

		Count
	};

	[Export]
	private int MinTimeMinutes = 0;
	[Export]
	private int MinTimeSeconds = 0;
	[Export]
	private int MinTimeMilliseconds = 0;

	private Thread ResourceLoadThread;
	private Player ThisPlayer;
	private Stopwatch Timer;

	private int TotalScore = 0;
	private int MaxCombo = 0;
	private ScoreBonus ExtraFlags = ScoreBonus.None;

	[Signal]
	public delegate void ResourcesLoadingFinishedEventHandler();

	private void OnEndOfChallengeReached() {
		Timer.Stop();

		//
		// calculate total score
		//
		TotalScore *= MaxCombo;

		int minutes = (int)Timer.Elapsed.TotalMinutes;
		int seconds = (int)Timer.Elapsed.TotalSeconds;
		int milliseconds = (int)Timer.Elapsed.TotalMilliseconds;

		ChallengeCache.UpdateScore( ChallengeCache.GetCurrentLeaderboard(), ChallengeCache.GetCurrentMap(), TotalScore, minutes, seconds, milliseconds );
	}
	public void EndCombo( int nScore ) {
		if ( nScore > MaxCombo ) {
			MaxCombo = nScore;
		}
	}
	public void IncreaseScore( int nAmount ) {

	}

	private void OnResourcesFinishedLoading() {
		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive );

		Console.PrintLine( "...Finished loading game" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
		
		Timer.Start();
	}
	public override void _Ready() {
		base._Ready();

		GetTree().CurrentScene = this;

		ThisPlayer = GetNode<Player>( "Players/Player0" );

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			ThisPlayer.SetupSplitScreen( 0 );
		}

		ResourceLoadThread = new Thread( () => { ResourceCache.Cache( this, null ); } );
		ResourceLoadThread.Start();

		ResourcesLoadingFinished += OnResourcesFinishedLoading;

		PhysicsServer2D.SetActive( true );

		EndOfChallenge end = GetNode<EndOfChallenge>( "EndOfChallenge" );
		end.Connect( "Triggered", Callable.From( OnEndOfChallengeReached ) );

		SetProcess( false );
		SetProcessInternal( false );

		//
		// force the game to run at the highest priority possible
		//
		using ( Process process = Process.GetCurrentProcess() ) {
			process.PriorityBoostEnabled = true;

			switch ( OS.GetName() ) {
			case "Linux":
			case "Windows":
				process.ProcessorAffinity = System.Environment.ProcessorCount;
				break;
			};

			process.PriorityClass = ProcessPriorityClass.AboveNormal;
		}
	}
};
