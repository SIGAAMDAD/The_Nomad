using Godot;

public partial class TitleMenu : Control {
	public enum MenuState {
		Main,
		Campaign,
		Coop,
		Multiplayer,
		Settings,
		Help,
		Mods
	};

	private AudioStreamPlayer UIChannel;
	private AudioStreamPlayer MusicTheme;

	private CoopMenu CoopMenu;
	private MultiplayerMenu MultiplayerMenu;
	private SettingsMenu SettingsMenu;
	private MainMenu MainMenu;
	private Button ExitButton;

	private LobbyBrowser LobbyBrowser;
	private LobbyFactory LobbyFactory;

	private AudioStream LoopingTheme;

	private MenuState State = MenuState.Main;

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
		case MenuState.Coop:
			CoopMenu.Hide();
			CoopMenu.SetProcess( false );
			CoopMenu.SetProcessInternal( false );
			CoopMenu.ProcessMode = ProcessModeEnum.Disabled;
			break;
		case MenuState.Multiplayer:
			MultiplayerMenu.Hide();
			LobbyBrowser.Hide();
			LobbyFactory.Hide();
			LobbyBrowser.SetProcess( false );
			LobbyBrowser.SetProcessInternal( false );
			LobbyBrowser.SetProcessUnhandledInput( false );
			LobbyBrowser.ProcessMode = ProcessModeEnum.Disabled;
			LobbyFactory.SetProcess( false );
			LobbyFactory.SetProcessInternal( false );
			LobbyFactory.SetProcessUnhandledInput( false );
			LobbyFactory.ProcessMode = ProcessModeEnum.Disabled;
			break;
		case MenuState.Settings:
			SettingsMenu.Hide();
			SettingsMenu.SetProcess( false );
			SettingsMenu.SetProcessInternal( true );
			SettingsMenu.SetProcessUnhandledInput( true );
			SettingsMenu.ProcessMode = ProcessModeEnum.Disabled;
			break;
		default:
			GD.PushError( "Invalid menu state!" );
			break;
		};

		MainMenu.SetProcess( true );
		MainMenu.SetProcessInternal( true );
		MainMenu.SetProcessUnhandledInput( true );
		MainMenu.ProcessMode = ProcessModeEnum.Always;

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
	private void OnMainMenuCoopMenu() {
		MainMenu.SetProcessUnhandledInput( false );
		CoopMenu.SetProcessUnhandledInput( true );
		CoopMenu.ProcessMode = ProcessModeEnum.Always;

		MainMenu.Hide();
		CoopMenu.Show();
		ExitButton.Show();
		State = MenuState.Coop;
	}
	private void OnMainMenuMultiplayerMenu() {
		MainMenu.SetProcessUnhandledInput( false );
		LobbyBrowser.SetProcessUnhandledInput( true );
		MultiplayerMenu.ProcessMode = ProcessModeEnum.Always;

		LobbyBrowser.ResetBrowser();

		MainMenu.Hide();
		MultiplayerMenu.Show();
		ExitButton.Show();
		LobbyBrowser.Show();
		State = MenuState.Multiplayer;
	}
	private void OnMainMenuSettingsMenu() {
		SettingsMenu.ProcessMode = ProcessModeEnum.Always;
		MainMenu.ProcessMode = ProcessModeEnum.Disabled;

		MainMenu.SetProcessUnhandledInput( false );
		SettingsMenu.SetProcess( true );

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

		MainMenu = GetNode<MainMenu>( "MainMenu" );
		MainMenu.SetProcess( true );
		MainMenu.SetProcessInternal( true );
		MainMenu.SetProcessUnhandledInput( true );
		MainMenu.Connect( "CoopMenu", Callable.From( OnMainMenuCoopMenu ) );
		MainMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		MainMenu.Connect( "MultiplayerMenu", Callable.From( OnMainMenuMultiplayerMenu ) );

		CoopMenu = GetNode<CoopMenu>( "CoopMenu" );
		CoopMenu.SetProcess( false );
		CoopMenu.SetProcessInternal( false );
		CoopMenu.SetProcessUnhandledInput( false );
		CoopMenu.ProcessMode = ProcessModeEnum.Disabled;

		MultiplayerMenu = GetNode<MultiplayerMenu>( "MultiplayerMenu" );
		MultiplayerMenu.SetProcess( false );
		MultiplayerMenu.SetProcessInternal( false );
		MultiplayerMenu.SetProcessUnhandledInput( false );
		MultiplayerMenu.ProcessMode = ProcessModeEnum.Disabled;

		SettingsMenu = GetNode<SettingsMenu>( "SettingsMenu" );
		SettingsMenu.SetProcess( false );
		SettingsMenu.SetProcessInternal( false );
		SettingsMenu.SetProcessUnhandledInput( false );
		SettingsMenu.ProcessMode = ProcessModeEnum.Disabled;

		ExitButton = GetNode<Button>( "ExitButton" );
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		LobbyBrowser = GetNode<LobbyBrowser>( "MultiplayerMenu/LobbyBrowser" );
		LobbyBrowser.SetProcess( false );
		LobbyBrowser.SetProcessInternal( false );
		LobbyBrowser.SetProcessUnhandledInput( false );

		LobbyFactory = GetNode<LobbyFactory>( "MultiplayerMenu/LobbyFactory" );
		LobbyFactory.SetProcess( false );
		LobbyFactory.SetProcessInternal( false );
		LobbyFactory.SetProcessUnhandledInput( false );

		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );
		UIChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		MusicTheme = GetNode<AudioStreamPlayer>( "Theme" );
		MusicTheme.VolumeDb = SettingsData.GetMusicVolumeLinear();
		MusicTheme.Connect( "finished", Callable.From( OnThemeIntroFinished ) );

		SettingsData.Instance.MusicVolumeChanged += () => { MusicTheme.VolumeDb = SettingsData.GetMusicVolumeLinear(); };
		SettingsData.Instance.EffectsVolumeChanged += () => { UIChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear(); };

		LoopingTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_loop2.ogg" );

		GetTree().CurrentScene = this;

		SetProcess( false );
		SetProcessInternal( false );
	}

	private void OnThemeIntroFinished() {
		Console.PrintLine( "Menu intro theme finished, moving to loop..." );

		MusicTheme.Stream = LoopingTheme;
		MusicTheme.Play();
		MusicTheme.Set( "parameters/looping", true );
		MusicTheme.SetProcess( false );
		MusicTheme.SetProcessInternal( false );
		MusicTheme.Disconnect( "finished", Callable.From( OnThemeIntroFinished ) );
	}
}
