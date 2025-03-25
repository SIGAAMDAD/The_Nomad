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
		GameConfiguration.LoadedLevel.Call( "ChangeScene" );
		QueueFree();

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
		GetNode( "/root/Console" ).Call( "print_line", "...Finished loading game", true );

		if ( SettingsData.GetNetworkingEnabled() ) {
			SteamLobby.Instance.SetProcess( true );
			SteamLobby.Instance.SetPhysicsProcess( true );
		}
	}
	private void OnFinishedLoadingScene() {
		Node scene = (Node)GameConfiguration.LoadedLevel.Get( "currentSceneNode" );
		scene.Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
	}
	
	private void OnTimerTimeout() {
		AdvanceTimer();
	}
	private void OnTransitionFinished() {
		Hide();
		GetNode( "/root/LoadingScreen" ).Call( "FadeIn" );
		GetNode( "/root/Console" ).Call( "print_line", "Loading game...", true );

		if ( SettingsData.GetNetworkingEnabled() ) {
			GetNode( "/root/Console" ).Call( "print_line", "Networking enabled, creating co-op lobby...", true );

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

		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels/world.tscn"
		);
		GameConfiguration.LoadedLevel = scene;

		scene.Connect( "OnComplete", Callable.From( OnFinishedLoadingScene ) );
	}

	public override void _Ready() {
		if ( SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

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

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnTransitionFinished ) );

		Author = GetNode<Label>( "VBoxContainer/AuthorName" );
		PressEnter = GetNode<Label>( "VBoxContainer/PressEnter" );
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