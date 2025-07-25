using Godot;
using System.Diagnostics;

public partial class Poem : Control {
	private static readonly Color DefaultColor = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	private static readonly Color HiddenColor = new Color( 0.0f, 0.0f, 0.0f, 0.0f );

	private bool Loading = false;

	private Label Author;
	private Label PressEnter;
	private Timer[] Timers;
	private Label[] Labels;
	private int CurrentTimer = 0;

	private PackedScene LoadedWorld = null;
	private System.Threading.Thread LoadThread = null;

	[Signal]
	public delegate void FinishedLoadingEventHandler();
	
	private void OnTimerTimeout() {
		AdvanceTimer();
	}

	private void OnFinishedLoading() {
		FinishedLoading -= OnFinishedLoading;

		LoadThread.Join();
		GetTree().ChangeSceneToPacked( LoadedWorld );
	}
	private void OnTransitionFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnTransitionFinished ) );

		Hide();
		GetNode<LoadingScreen>( "/root/LoadingScreen" ).Call( "FadeIn" );

		UIAudioManager.FadeMusic();

		World.LoadTime = Stopwatch.StartNew();

		/*
		if ( SettingsData.GetNetworkingEnabled() ) {
			Console.PrintLine( "Networking enabled, creating co-op lobby..." );

			GameConfiguration.GameMode = GameMode.Online;

			SteamLobby.Instance.SetMaxMembers( 4 );
			string name = SteamManager.GetSteamName();
			if ( name[ name.Length - 1 ] == 's' ) {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}' Lobby", name ) );
			} else {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}'s Lobby", name ) );
			}

			SteamLobby.Instance.CreateLobby();
		} else {
			GameConfiguration.GameMode = GameMode.SinglePlayer;
		}
		*/

		GameConfiguration.GameMode = GameMode.SinglePlayer;

		Console.PrintLine( "Loading game..." );

		FinishedLoading += OnFinishedLoading;
		LoadThread = new System.Threading.Thread( () => {
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( MethodName.EmitSignal, SignalName.FinishedLoading );
		} );
		LoadThread.Start();
	}

	public override void _Ready() {
		Theme = AccessibilityManager.DefaultTheme;

		base._Ready();

		Timers = [
			GetNode<Timer>( "VBoxContainer/Label/Timer1" ),
			GetNode<Timer>( "VBoxContainer/Label2/Timer2" ),
			GetNode<Timer>( "VBoxContainer/Label3/Timer3" ),
			GetNode<Timer>( "VBoxContainer/Label4/Timer4" ),
			GetNode<Timer>( "VBoxContainer/Label5/Timer5" )
		];
		Labels = [
			GetNode<Label>( "VBoxContainer/Label" ),
			GetNode<Label>( "VBoxContainer/Label2" ),
			GetNode<Label>( "VBoxContainer/Label3" ),
			GetNode<Label>( "VBoxContainer/Label4" ),
			GetNode<Label>( "VBoxContainer/Label5" )
		];

		for ( int i = 0; i < Timers.Length; i++ ) {
			Timers[i].Connect( "timeout", Callable.From( OnTimerTimeout ) );
		}

		Tween FadeTween = CreateTween();
		FadeTween.TweenProperty( Labels[ CurrentTimer ], "modulate", DefaultColor, 1.5f );

		Author = GetNode<Label>( "VBoxContainer/AuthorName" );
		PressEnter = GetNode<Label>( "VBoxContainer/PressEnter" );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		if ( Input.IsActionJustPressed( "ui_advance" ) ) {
			CallDeferred( MethodName.AdvanceTimer );
		}
	}

	private void AdvanceTimer() {
		if ( Loading ) {
			return;
		}
		if ( CurrentTimer >= Labels.Length ) {
			Loading = true;
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnTransitionFinished ) );
			GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
			return;
		}

		CurrentTimer++;
		if ( CurrentTimer < Timers.Length ) {
			Tween FadeTween = CreateTween();
			FadeTween.TweenProperty( Labels[ CurrentTimer ], "modulate", DefaultColor, 1.5f );

			Timers[ CurrentTimer ].Start();
		} else {
			Tween FadeTween = CreateTween();
			FadeTween.TweenProperty( Author, "modulate", DefaultColor, 1.5f );
			FadeTween.TweenProperty( PressEnter, "modulate", DefaultColor, 0.9f );
		}
	}
};