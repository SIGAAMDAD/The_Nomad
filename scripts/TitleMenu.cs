using System.Xml;
using Godot;

public partial class TitleMenu : Control {
	public enum MenuState {
		Main,
		Campaign,
		Extras,
		Settings,
		Help,
		Mods
	};

	private AudioStreamPlayer UIChannel;
	private AudioStreamPlayer MusicTheme;

	private ExtrasMenu ExtrasMenu;
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
		case MenuState.Extras:
			CoopMenu.Hide();
			CoopMenu.SetProcess( false );
			CoopMenu.SetProcessInternal( false );

			MultiplayerMenu.Hide();
			LobbyBrowser.Hide();
			LobbyFactory.Hide();
			LobbyBrowser.SetProcess( false );
			LobbyBrowser.SetProcessInternal( false );
			LobbyBrowser.SetProcessUnhandledInput( false );
			LobbyFactory.SetProcess( false );
			LobbyFactory.SetProcessInternal( false );
			LobbyFactory.SetProcessUnhandledInput( false );

			ExtrasMenu.Hide();
			ExtrasMenu.SetProcess( false );
			ExtrasMenu.SetProcessInternal( false );
			ExtrasMenu.ProcessMode = ProcessModeEnum.Disabled;
			break;
		/*
		case MenuState.Campaign:
			CampaignMenu.Hide();
			CampaignMenu.SetProcess( false );
			CampaignMenu.SetProcessInternal( false );
			CampaignMenu.SetProcessUnhandledInput( false );
			break;
			*/
		case MenuState.Settings:
			SettingsMenu.Hide();
			SettingsMenu.SetProcess( false );
			SettingsMenu.SetProcessInternal( true );
			SettingsMenu.SetProcessUnhandledInput( true );
			SettingsMenu.ProcessMode = ProcessModeEnum.Disabled;
			break;
		default:
			Console.PrintError( "Invalid menu state!" );
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
	private void OnMainMenuExtrasMenu() {
		MainMenu.SetProcessUnhandledInput( false );
		ExtrasMenu.ProcessMode = ProcessModeEnum.Always;

		MainMenu.Hide();
		ExtrasMenu.Show();
		ExtrasMenu.Reset();
		ExitButton.Show();
		State = MenuState.Extras;
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
		MainMenu.Connect( "ExtrasMenu", Callable.From( OnMainMenuExtrasMenu ) );
		MainMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );

		ExtrasMenu = GetNode<ExtrasMenu>( "ExtrasMenu" );
		ExtrasMenu.SetProcess( false );
		ExtrasMenu.SetProcessInternal( false );
		ExtrasMenu.SetProcessUnhandledInput( false );
		ExtrasMenu.ProcessMode = ProcessModeEnum.Inherit;

		CoopMenu = GetNode<CoopMenu>( "ExtrasMenu/CoopMenu" );
		CoopMenu.SetProcess( false );
		CoopMenu.SetProcessInternal( false );
		CoopMenu.SetProcessUnhandledInput( false );
		CoopMenu.ProcessMode = ProcessModeEnum.Inherit;

		MultiplayerMenu = GetNode<MultiplayerMenu>( "ExtrasMenu/MultiplayerMenu" );
		MultiplayerMenu.SetProcess( false );
		MultiplayerMenu.SetProcessInternal( false );
		MultiplayerMenu.SetProcessUnhandledInput( false );
		MultiplayerMenu.ProcessMode = ProcessModeEnum.Inherit;

		SettingsMenu = GetNode<SettingsMenu>( "SettingsMenu" );
		SettingsMenu.SetProcess( false );
		SettingsMenu.SetProcessInternal( false );
		SettingsMenu.SetProcessUnhandledInput( false );
		SettingsMenu.ProcessMode = ProcessModeEnum.Disabled;

		ExitButton = GetNode<Button>( "ExitButton" );
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		LobbyBrowser = GetNode<LobbyBrowser>( "ExtrasMenu/MultiplayerMenu/LobbyBrowser" );
		LobbyBrowser.SetProcess( false );
		LobbyBrowser.SetProcessInternal( false );
		LobbyBrowser.SetProcessUnhandledInput( false );

		LobbyFactory = GetNode<LobbyFactory>( "ExtrasMenu/MultiplayerMenu/LobbyFactory" );
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
