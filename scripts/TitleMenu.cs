using Godot;

public partial class TitleMenu : Control {
	public enum MenuState {
		Main,
		Campaign,
		Multiplayer,
		Settings,
		Help,
		Mods
	};

	private AudioStreamPlayer UIChannel;
	private AudioStreamPlayer MusicTheme;

	private Control MultiplayerMenu;
	private Control SettingsMenu;
	private Control MainMenu;
	private Button ExitButton;

	private Control LobbyBrowser;
	private Control LobbyFactory;

	private AudioStream LoopingTheme;

	private MenuState State = MenuState.Main;

	public override void _ExitTree() {
		base._ExitTree();

		LobbyBrowser.QueueFree();
		LobbyFactory.QueueFree();

		MultiplayerMenu.QueueFree();
		SettingsMenu.QueueFree();
		MainMenu.QueueFree();
		ExitButton.QueueFree();

		UIChannel.QueueFree();

//		MainMenu.Disconnect( "CampaignMenu", Callable.From( OnMainMenuCampaignMenu ) );
		MainMenu.Disconnect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		MainMenu.Disconnect( "MultiplayerMenu", Callable.From( OnMainMenuMultiplayerMenu ) );

		QueueFree();
	}

    private void OnExitButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		switch ( State ) {
		/*
		case MenuState.Campaign:
			CampaignMenu.Hide();
			CampaignMenu.SetProcess( false );
			CampaignMenu.SetProcessInternal( false );
			CampaignMenu.SetProcessUnhandledInput( false );
			break;
			*/
		case MenuState.Multiplayer:
			MultiplayerMenu.Hide();
			LobbyBrowser.Hide();
			LobbyFactory.Hide();
			LobbyBrowser.SetProcess( false );
			LobbyBrowser.SetProcessInternal( false );
			LobbyBrowser.SetProcessUnhandledInput( false );
			LobbyFactory.SetProcess( false );
			LobbyFactory.SetProcessInternal( false );
			LobbyFactory.SetProcessUnhandledInput( false );
			break;
		case MenuState.Settings:
			SettingsMenu.Hide();
			SettingsMenu.SetProcess( false );
			SettingsMenu.SetProcessInternal( true );
			SettingsMenu.SetProcessUnhandledInput( true );
			break;
		default:
			GD.PushError( "Invalid menu state!" );
			break;
		};

		MainMenu.SetProcess( true );
		MainMenu.SetProcessInternal( true );
		MainMenu.SetProcessUnhandledInput( true );

		ExitButton.Hide();
		MainMenu.Show();
		State = MenuState.Main;
	}
	/*
	private void OnMainMenuCampaignMenu() {
		MainMenu.SetProcess( false );
		MainMenu.SetProcessInternal( false );
		MainMenu.SetProcessUnhandledInput( false );
		CampaignMenu.SetProcess( true );
		CampaignMenu.SetProcessInternal( true );
		CampaignMenu.SetProcessUnhandledInput( true );

		MainMenu.Hide();
		CampaignMenu.Show();
		ExitButton.Show();
		State = MenuState.Campaign;
	}
	*/
	private void OnMainMenuMultiplayerMenu() {
		MainMenu.SetProcess( false );
		MainMenu.SetProcessInternal( false );
		MainMenu.SetProcessUnhandledInput( false );
		LobbyBrowser.SetProcess( true );
		LobbyBrowser.SetProcessInternal( true );
		LobbyBrowser.SetProcessUnhandledInput( true );

		MainMenu.Hide();
		MultiplayerMenu.Show();
		ExitButton.Show();
		LobbyBrowser.Show();
		State = MenuState.Multiplayer;
	}
	private void OnMainMenuSettingsMenu() {
		MainMenu.SetProcess( false );
		MainMenu.SetProcessInternal( false );
		MainMenu.SetProcessUnhandledInput( false );
		SettingsMenu.SetProcess( true );
		SettingsMenu.SetProcessInternal( true );
		SettingsMenu.SetProcessUnhandledInput( true );

		MainMenu.Hide();
		SettingsMenu.Show();
		ExitButton.Show();
		State = MenuState.Settings;
	}

	public override void _Ready() {
		PhysicsServer2D.SetActive( false );

		Control Background = GetNode<Control>( "MenuBackground" );
		Background.SetProcess( false );
		Background.SetProcessInternal( false );

		MainMenu = GetNode<Control>( "MainMenu" );
		MainMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		MainMenu.Connect( "MultiplayerMenu", Callable.From( OnMainMenuMultiplayerMenu ) );

		MultiplayerMenu = GetNode<Control>( "MultiplayerMenu" );
		MultiplayerMenu.SetProcess( false );
		MultiplayerMenu.SetProcessInternal( false );
		MultiplayerMenu.SetProcessUnhandledInput( false );

		SettingsMenu = GetNode<Control>( "SettingsMenu" );
		SettingsMenu.SetProcess( false );
		SettingsMenu.SetProcessInternal( false );
		SettingsMenu.SetProcessUnhandledInput( false );

		ExitButton = GetNode<Button>( "ExitButton" );
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		LobbyBrowser = GetNode<Control>( "MultiplayerMenu/LobbyBrowser" );
		LobbyBrowser.SetProcess( false );
		LobbyBrowser.SetProcessInternal( false );
		LobbyBrowser.SetProcessUnhandledInput( false );

		LobbyFactory = GetNode<Control>( "MultiplayerMenu/LobbyFactory" );
		LobbyFactory.SetProcess( false );
		LobbyFactory.SetProcessInternal( false );
		LobbyFactory.SetProcessUnhandledInput( false );

		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );

		MusicTheme = GetNode<AudioStreamPlayer>( "Theme" );
		MusicTheme.SetProcess( false );
		MusicTheme.Connect( "finished", Callable.From( OnThemeIntroFinished ) );

		LoopingTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_loop2.ogg" );

		GetTree().CurrentScene = this;

		SetProcess( false );
	}

	private void OnThemeIntroFinished() {
		MusicTheme.Stream = LoopingTheme;
		MusicTheme.Play();
		MusicTheme.Set( "parameters/looping", true );
		MusicTheme.Disconnect( "finished", Callable.From( OnThemeIntroFinished ) );
	}
}
