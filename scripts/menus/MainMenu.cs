using System.Threading;
using Godot;

public partial class MainMenu : Control {
	[Signal]
	public delegate void SettingsMenuEventHandler();
	[Signal]
	public delegate void HelpMenuEventHandler();
	[Signal]
	public delegate void MultiplayerMenuEventHandler();
	[Signal]
	public delegate void ModsMenuEventHandler();
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private enum IndexedButton : int {
		Story,
		Multiplayer,
		Settings,
		Mods,
		Quit,

		Count
	};

	private static Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private static Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	private Button[] ButtonList = null;
	private int ButtonIndex = 0;
	private bool Loaded = false;

	private static PackedScene LoadedWorld = null;
	private static Thread LoadThread = null;
	private static object LoadSync = new object();

	private AudioStreamPlayer UIChannel;
	private CanvasLayer TransitionScreen;

	private static Tween AudioFade;

	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
		AudioFade.Finished -= OnAudioFadeFinished;
	}
	private void OnFinishedLoading() {
		LoadThread.Join();
		GetTree().ChangeSceneToPacked( LoadedWorld );
		QueueFree();
	}
	private void OnBeginGameFinished() {
		TransitionScreen.Disconnect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
		QueueFree();
	}

	private void OnContinueGameButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;
		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		TransitionScreen.Call( "transition" );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 2.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

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

		ArchiveSystem.LoadGame();

		Console.PrintLine( "Loading game..." );

		Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
		LoadThread = new Thread( () => {
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	private void OnNewGameButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		TransitionScreen.Call( "transition" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		GameConfiguration.GameDifficulty = GameDifficulty.Intended;
	}

	private void OnSettingsButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		EmitSignal( "SettingsMenu" );
	}
	private void OnModsButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		EmitSignal( "ModsMenu" );
	}
	private void OnMultiplayerButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		EmitSignal( "MultiplayerMenu" );
	}
	private void OnQuitGameButtonPressed() {
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
		if ( SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		Button NewGameButton = GetNode<Button>( "VBoxContainer/NewGameButton" );
		NewGameButton.SetProcess( false );
		NewGameButton.SetProcessInternal( false );
		NewGameButton.Visible = !ArchiveSystem.Instance.IsLoaded();
		NewGameButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		NewGameButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		NewGameButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		NewGameButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		NewGameButton.Connect( "pressed", Callable.From( OnNewGameButtonPressed ) );
		NewGameButton.Scale = GetTree().Root.ContentScaleSize;
		
		Button ContinueGameButton = GetNode<Button>( "VBoxContainer/ContinueGameButton" );
		ContinueGameButton.SetProcess( false );
		ContinueGameButton.SetProcessInternal( false );
		ContinueGameButton.Visible = ArchiveSystem.Instance.IsLoaded();
		ContinueGameButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		ContinueGameButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		ContinueGameButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		ContinueGameButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		ContinueGameButton.Connect( "pressed", Callable.From( OnContinueGameButtonPressed ) );
		ContinueGameButton.Scale = GetTree().Root.ContentScaleSize;

		Button MultiplayerButton = GetNode<Button>( "VBoxContainer/MultiplayerButton" );
		MultiplayerButton.SetProcess( false );
		MultiplayerButton.SetProcessInternal( false );
		MultiplayerButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Multiplayer ); } ) );
		MultiplayerButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Multiplayer ); } ) );
		MultiplayerButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Multiplayer ); } ) );
		MultiplayerButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Multiplayer ); } ) );
		MultiplayerButton.Connect( "pressed", Callable.From( OnMultiplayerButtonPressed ) );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.SetProcess( false );
		SettingsButton.SetProcessInternal( false );
		SettingsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "pressed", Callable.From( OnSettingsButtonPressed ) );

		Button ModsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		ModsButton.SetProcess( false );
		ModsButton.SetProcessInternal( false );
		ModsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "pressed", Callable.From( OnModsButtonPressed ) );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		ExitButton.SetProcess( false );
		ExitButton.SetProcessInternal( false );
		ExitButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Quit ); } ) );
		ExitButton.Connect( "pressed", Callable.From( OnQuitGameButtonPressed ) );

		Label AppVersion = GetNode<Label>( "AppVersion" );
		AppVersion.SetProcess( false );
		AppVersion.SetProcessInternal( false );
		AppVersion.Text = "App Version " + (string)ProjectSettings.GetSetting( "application/config/version" );

		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		TransitionScreen.SetProcess( false );
		TransitionScreen.SetProcessInternal( false );

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			ButtonList = [
				ContinueGameButton,
				MultiplayerButton,
				SettingsButton,
				ModsButton,
				ExitButton
			];
			OnButtonFocused( 0 );
		} else {
			ButtonList = [
				NewGameButton,
				MultiplayerButton,
				SettingsButton,
				ModsButton,
				ExitButton
			];
			OnButtonFocused( 0 );
		}
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
