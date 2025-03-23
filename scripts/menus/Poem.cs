using Godot;

public partial class Poem : Control {
	private bool Loading = false;

	private CanvasLayer TransitionScreen;
	private Label Author;
	private Label PressEnter;
	private System.Collections.Generic.List<Timer> Timers;
	private System.Collections.Generic.List<Label> Labels;
	private int CurrentTimer = 0;

	private void OnFinishedLoading() {
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}
	private void OnFinishedLoadingScene() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );
		QueueFree();

		Node scene = (Node)( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Get( "currentSceneNode" );
		scene.Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
	}
	
	private void OnTimerTimeout() {
		AdvanceTimer();
	}
	private void OnTransitionFinished() {
		Hide();
		GetNode( "/root/LoadingScreen" ).Call( "FadeIn" );
		GetNode( "/root/Console" ).Call( "print_line", "Loading game..." );

		SteamLobby.Instance.SetMaxMembers( 4 );
		SteamLobby.Instance.CreateLobby();
		SteamLobby.Instance.SetLobbyName( SteamManager.GetSteamName() + "'s World" );

		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels/world.tscn"
		);
		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );

		scene.Connect( "OnComplete", Callable.From( OnFinishedLoadingScene ) );
	}

	public override void _Ready() {
		base._Ready();

		Timers = new System.Collections.Generic.List<Timer>{
			GetNode<Timer>( "VBoxContainer/Label/Timer1" ),
			GetNode<Timer>( "VBoxContainer/Label2/Timer2" ),
			GetNode<Timer>( "VBoxContainer/Label3/Timer3" ),
			GetNode<Timer>( "VBoxContainer/Label4/Timer4" ),
			GetNode<Timer>( "VBoxContainer/Label5/Timer5" )
		};
		Labels = new System.Collections.Generic.List<Label>{
			GetNode<Label>( "VBoxContainer/Label" ),
			GetNode<Label>( "VBoxContainer/Label2" ),
			GetNode<Label>( "VBoxContainer/Label3" ),
			GetNode<Label>( "VBoxContainer/Label4" ),
			GetNode<Label>( "VBoxContainer/Label5" )
		};

		for ( int i = 0; i < Timers.Count; i++ ) {
			Timers[i].Connect( "timeout", Callable.From( OnTimerTimeout ) );
		}
		for ( int i = 0; i < Labels.Count; i++ ) {
			if ( SettingsData.GetDyslexiaMode() ) {
				Labels[i].Theme = AccessibilityManager.DyslexiaTheme;
			} else {
				Labels[i].Theme = AccessibilityManager.DefaultTheme;
			}
		}

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnTransitionFinished ) );

		Author = GetNode<Label>( "VBoxContainer/AuthorName" );
		if ( SettingsData.GetDyslexiaMode() ) {
			Author.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Author.Theme = AccessibilityManager.DefaultTheme;
		}

		PressEnter = GetNode<Label>( "VBoxContainer/PressEnter" );
		if ( SettingsData.GetDyslexiaMode() ) {
			PressEnter.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			PressEnter.Theme = AccessibilityManager.DefaultTheme;
		}
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		if ( Input.IsActionJustPressed( "ui_advance" ) ) {
			CallDeferred( "AdvanceTimer" );
		}
	}

	private void AdvanceTimer() {
		if ( Loading ) {
			return;
		}
		if ( CurrentTimer >= Labels.Count ) {
			Loading = true;
			TransitionScreen.Call( "transition" );
			return;
		}

		CurrentTimer++;
		if ( CurrentTimer < Timers.Count ) {
			Labels[ CurrentTimer ].Show();
			Timers[ CurrentTimer ].Start();
			if ( CurrentTimer == Timers.Count - 1 ) {
				Author.Show();
				PressEnter.Show();
			}
		}
	}
};