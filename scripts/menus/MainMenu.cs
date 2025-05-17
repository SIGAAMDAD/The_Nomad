using System.Threading;
using Godot;

public partial class MainMenu : Control {
	private enum IndexedButton : int {
		Story,
		Extras,
		Settings,
		Mods,
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

	private AudioStreamPlayer UIChannel;

	private static Tween AudioFade;

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

	private void OnContinueGameFinished() {
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Disconnect( "transition_finished", Callable.From(  OnContinueGameFinished ) );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Console.PrintLine( "Loading game..." );

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

		FinishedLoading += () => {
			LoadThread.Join();
			GetTree().ChangeSceneToPacked( LoadedWorld );
		};
		LoadThread = new Thread( () => {
			ArchiveSystem.LoadGame( SettingsData.GetSaveSlot() );
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	private void OnContinueGameButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;

		EmitSignalBeginGame();
		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();

		Hide();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From(  OnContinueGameFinished ) );
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Call( "transition" );

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 2.5f );
		AudioFade.Connect( "finished", Callable.From(  OnAudioFadeFinished ) );
	}
	private void OnNewGameButtonPressed() {
		if ( Loaded ) {
			return;
		}
		Loaded = true;

		int SlotIndex = 0;
		while ( ArchiveSystem.SlotExists( SlotIndex ) ) {
			SlotIndex++;
		}
		SettingsData.SetSaveSlot( SlotIndex );

		EmitSignalBeginGame();

		AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From(  OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		GetNode<CanvasLayer>( "/root/TransitionScreen" ).Connect( "transition_finished", Callable.From(  OnBeginGameFinished ) );
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
	private void OnModsButtonPressed() {
		if ( Loaded ) {
			return;
		}
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		EmitSignalModsMenu();
	}
	private void OnExtrasButtonPressed() {
		if ( Loaded ) {
			return;
		}
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		EmitSignalExtrasMenu();
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

		if (SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		MultiplayerMapManager.Init();
		ArchiveSystem.Instance.CheckSaveData();

		ProcessMode = ProcessModeEnum.Always;

		Button StoryModeButton = GetNode<Button>( "VBoxContainer/StoryModeButton" );
		StoryModeButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		StoryModeButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		StoryModeButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		StoryModeButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		if ( !ArchiveSystem.Instance.IsLoaded() ) {
			StoryModeButton.Text = TranslationServer.Translate( "NEW_GAME" );
			StoryModeButton.Connect( "pressed", Callable.From( OnNewGameButtonPressed ) );
		} else {
			StoryModeButton.Text = TranslationServer.Translate( "CONTINUE_GAME" );
			StoryModeButton.Connect( "pressed", Callable.From( OnContinueGameButtonPressed ) );
		}

		Button ExtrasButton = GetNode<Button>( "VBoxContainer/ExtrasButton" );
		ExtrasButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Extras ); } ) );
		ExtrasButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Extras ); } ) );
		ExtrasButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Extras ); } ) );
		ExtrasButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Extras ); } ) );
		ExtrasButton.Connect( "pressed", Callable.From( OnExtrasButtonPressed ) );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Settings ); } ) );
		SettingsButton.Connect( "pressed", Callable.From( OnSettingsButtonPressed ) );

		Button ModsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		ModsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( (int)IndexedButton.Mods ); } ) );
		ModsButton.Connect( "pressed", Callable.From( OnModsButtonPressed ) );

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
		AppVersion.Text = "App Version " + (string)ProjectSettings.GetSetting( "application/config/version" );
		AppVersion.ProcessMode = ProcessModeEnum.Disabled;

		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );

		ButtonList = [
			StoryModeButton,
			ExtrasButton,
			SettingsButton,
			ModsButton,
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
