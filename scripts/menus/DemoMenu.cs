#if DEMO
using System.Threading;
using ChallengeMode;
using Godot;

public partial class DemoMenu : Control {
	private enum IndexedButton : int {
		Play,
		JohnWick,
		Settings,
		Credits,
		Quit,

		Count
	};

	private static Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private static Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	private Button[] ButtonList = null;
	private int ButtonIndex = 0;
	private bool Loaded = false;

	private PackedScene LoadedWorld;
	private Thread LoadThread;

	[Signal]
	public delegate void BeginGameEventHandler();
	[Signal]
	public delegate void SettingsMenuEventHandler();
	[Signal]
	public delegate void HelpMenuEventHandler();
	[Signal]
	public delegate void ExtrasMenuEventHandler();
	[Signal]
	public delegate void ModsMenuEventHandler();
	[Signal]
	public delegate void CreditsMenuEventHandler();
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
		AudioFade.Finished -= OnAudioFadeFinished;
	}
	private void OnBeginGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From(  OnBeginGameFinished ) );
		QueueFree();
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
	}

	private void OnStoryModeMapFinishedLoading( PackedScene mapData, Resource quest ) {
		Resource questData = Questify.Instantiate( quest );
		ChallengeCache.SetQuestData( questData );

		GameConfiguration.GameMode = GameMode.ChallengeMode;

		QueueFree();
		GetTree().ChangeSceneToPacked( mapData );
	}
	private void OnPlayButtonPressedFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnPlayButtonPressedFinished ) );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

		ChallengeCache.SetCurrentMap( 0 );
		ChallengeCache.MapList[ 0 ].Load( OnStoryModeMapFinishedLoading );
	}
	private void OnPlayButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnPlayButtonPressedFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
	}

	private void OnJohnWickModeFadeFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From( OnJohnWickModeFadeFinished ) );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

		FinishedLoading += () => {
			LoadThread.Join();
			GameConfiguration.GameMode = GameMode.JohnWick;
			GetTree().ChangeSceneToPacked( LoadedWorld );
		};
		LoadThread = new Thread( () => {
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/john_wick_mode.tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	private void OnJohnWickModeButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From( OnJohnWickModeFadeFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );
	}
	private void OnSettingsButtonPressed() {
		if ( Loaded ) {
			return;
		}
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		EmitSignalSettingsMenu();
	}
	private void OnCreditsButtonPressed() {
		if ( Loaded ) {
			return;
		}
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		EmitSignalCreditsMenu();
	}
	private void OnQuitGameButtonPressed() {
		if ( Loaded ) {
			return;
		}
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		GetTree().Quit();
	}

	private void OnButtonFocused( int nButtonIndex ) {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();

		ButtonIndex = nButtonIndex;
		ButtonList[ ButtonIndex ].Modulate = Selected;
	}
	private void OnButtonUnfocused( int nButtonIndex ) {
		ButtonList[ ButtonIndex ].Modulate = Unselected;
	}

	public override void _Ready() {
		PhysicsServer2D.SetActive( false );

		if ( SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		ChallengeCache.Init();
		ChallengeCache.GetScore( 0, out int score, out int minutes, out int seconds, out int milliseconds, null );

		ProcessMode = ProcessModeEnum.Always;

		Button PlayButton = GetNode<Button>( "VBoxContainer/StartGameButton" );
		PlayButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Play ); } ) );
		PlayButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Play ); } ) );
		PlayButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Play ); } ) );
		PlayButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Play ); } ) );
		PlayButton.Connect( "pressed", Callable.From( OnPlayButtonPressed ) );

		Button JohnWickModeButton = GetNode<Button>( "VBoxContainer/JohnWickButton" );
		JohnWickModeButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.JohnWick ); } ) );
		JohnWickModeButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.JohnWick ); } ) );
		JohnWickModeButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.JohnWick ); } ) );
		JohnWickModeButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.JohnWick ); } ) );
		JohnWickModeButton.Connect( "pressed", Callable.From( OnJohnWickModeButtonPressed ) );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "pressed", Callable.From( OnSettingsButtonPressed ) );

		Button CreditsButton = GetNode<Button>( "VBoxContainer/CreditsButton" );
		CreditsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Credits ); } ) );
		CreditsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Credits ); } ) );
		CreditsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Credits ); } ) );
		CreditsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Credits ); } ) );
		CreditsButton.Connect( "pressed", Callable.From( OnCreditsButtonPressed ) );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		ExitButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "pressed", Callable.From( OnQuitGameButtonPressed ) );

		Label AppVersion = GetNode<Label>( "AppVersion" );
		AppVersion.Text = "App Version " + (string)ProjectSettings.GetSetting( "application/config/version" ) + " (DEMO)";
		AppVersion.ProcessMode = ProcessModeEnum.Disabled;

		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );

		ButtonList = [
			PlayButton,
			JohnWickModeButton,
			SettingsButton,
			CreditsButton,
			ExitButton
		];
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustPressed( "ui_down" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "focus_exited" );
			if ( ButtonIndex == ButtonList.Length - 1 ) {
				ButtonIndex = 0;
			} else {
				ButtonIndex++;
			}
			ButtonList[ ButtonIndex ].EmitSignal( "focus_entered" );
		}
		else if ( Input.IsActionJustPressed( "ui_up" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "focus_exited" );
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Length - 1;
			} else {
				ButtonIndex--;
			}
			ButtonList[ ButtonIndex ].EmitSignal( "focus_entered" );
		}
		else if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "focus_entered" );
			ButtonList[ ButtonIndex ].CallDeferred( "emit_signal", "pressed" );
		}
	}
};
#endif