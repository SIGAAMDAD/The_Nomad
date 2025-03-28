using System.Threading;
using Godot;

public partial class MainMenu : Control {
	[Signal]
	public delegate void CampaignMenuEventHandler();
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

	private static Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private static Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	private Button[] ButtonList = null;
	private int ButtonIndex = 0;
	private bool Loaded = false;

	private PackedScene LoadedWorld = null;
	private Thread LoadThread = null;

	private AudioStreamPlayer UIChannel;
	private CanvasLayer TransitionScreen;

	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
	}
	private void OnFinishedLoading() {
		LoadThread.Join();
		GetTree().ChangeSceneToPacked( LoadedWorld );

		ButtonList = null;
		LoadThread = null;
		LoadedWorld = null;
	}
	private void LoadGame() {
		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

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

		Tween AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 2.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
		LoadThread = new Thread( () => {
			LoadedWorld = ResourceLoader.Load<PackedScene>( "res://levels/world.tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	private void OnContinueGameFinished() {
		LoadGame();
	}

    private void OnCampaignButtonPressed() {
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			if ( Loaded ) {
				return;
			}
			Loaded = true;
			UIChannel.Stream = UISfxManager.BeginGame;
			UIChannel.Play();
			TransitionScreen.Call( "transition" );
			TransitionScreen.Connect( "transition_finished", Callable.From( OnContinueGameFinished ) );
		} else {
			UIChannel.Stream = UISfxManager.ButtonPressed;
			UIChannel.Play();
			EmitSignal( "CampaignMenu" );
		}
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

		Button CampaignButton = GetNode<Button>( "VBoxContainer/CampaignButton" );
		CampaignButton.SetProcess( false );
		CampaignButton.SetProcessInternal( false );
		CampaignButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		CampaignButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		CampaignButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 0 ); } ) );
		CampaignButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 0 ); } ) );
		CampaignButton.Connect( "pressed", Callable.From( OnCampaignButtonPressed ) );
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			CampaignButton.Text = "CONTINUE_STORY_BUTTON";
		}

		Button MultiplayerButton = GetNode<Button>( "VBoxContainer/MultiplayerButton" );
		MultiplayerButton.SetProcess( false );
		MultiplayerButton.SetProcessInternal( false );
		MultiplayerButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 1 ); } ) );
		MultiplayerButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 1 ); } ) );
		MultiplayerButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 1 ); } ) );
		MultiplayerButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 1 ); } ) );
		MultiplayerButton.Connect( "pressed", Callable.From( OnMultiplayerButtonPressed ) );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.SetProcess( false );
		SettingsButton.SetProcessInternal( false );
		SettingsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 2 ); } ) );
		SettingsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 2 ); } ) );
		SettingsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 2 ); } ) );
		SettingsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 2 ); } ) );
		SettingsButton.Connect( "pressed", Callable.From( OnSettingsButtonPressed ) );

		Button ModsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		ModsButton.SetProcess( false );
		ModsButton.SetProcessInternal( false );
		ModsButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 3 ); } ) );
		ModsButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 3 ); } ) );
		ModsButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 3 ); } ) );
		ModsButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 3 ); } ) );
		ModsButton.Connect( "pressed", Callable.From( OnModsButtonPressed ) );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		ExitButton.SetProcess( false );
		ExitButton.SetProcessInternal( false );
		ExitButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( 4 ); } ) );
		ExitButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( 4 ); } ) );
		ExitButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( 4 ); } ) );
		ExitButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( 4 ); } ) );
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

		CampaignButton.Modulate = Selected;
		ButtonList = [
			CampaignButton,
			MultiplayerButton,
			SettingsButton,
			ModsButton,
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
