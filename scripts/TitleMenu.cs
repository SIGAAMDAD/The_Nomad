using Godot;
using Steamworks;

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

	private Control CampaignMenu;
	private Control MultiplayerMenu;
	private Control SettingsMenu;
	private Control MainMenu;
	private Button ExitButton;

	private Control SaveSlotSelect;
	private Control DifficultySelect;

	private Control LobbyBrowser;
	private Control LobbyFactory;

	private AudioStream LoopingTheme;

	private MenuState State = MenuState.Main;

	private void OnExitButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		switch ( State ) {
		case MenuState.Campaign:
			CampaignMenu.Hide();
			CampaignMenu.SetProcess( false );
			CampaignMenu.SetProcessInternal( false );
			break;
		case MenuState.Multiplayer:
			MultiplayerMenu.Hide();
			LobbyBrowser.Hide();
			LobbyFactory.Hide();
			LobbyBrowser.SetProcess( false );
			LobbyBrowser.SetProcessInternal( false );
			LobbyFactory.SetProcess( false );
			LobbyFactory.SetProcessInternal( false );
			break;
		case MenuState.Settings:
			SettingsMenu.Hide();
			SettingsMenu.SetProcess( false );
			SettingsMenu.SetProcessInternal( true );
			break;
		default:
			GD.PushError( "Invalid menu state!" );
			break;
		};

		MainMenu.SetProcess( true );
		MainMenu.SetProcessInternal( true );

		ExitButton.Hide();
		MainMenu.Show();
		State = MenuState.Main;
	}
	private void OnMainMenuCampaignMenu() {
		MainMenu.SetProcess( false );
		MainMenu.SetProcessInternal( false );
		CampaignMenu.SetProcess( true );
		CampaignMenu.SetProcessInternal( true );

		MainMenu.Hide();
		CampaignMenu.Show();
		ExitButton.Show();
		State = MenuState.Campaign;
	}
	private void OnMainMenuMultiplayerMenu() {
		MainMenu.SetProcess( false );
		MainMenu.SetProcessInternal( false );
		LobbyBrowser.SetProcess( true );
		LobbyBrowser.SetProcessInternal( true );

		MainMenu.Hide();
		MultiplayerMenu.Show();
		ExitButton.Show();
		LobbyBrowser.Show();
		State = MenuState.Multiplayer;
	}
	private void OnMainMenuSettingsMenu() {
		MainMenu.SetProcess( false );
		MainMenu.SetProcessInternal( false );
		SettingsMenu.SetProcess( true );
		SettingsMenu.SetProcessInternal( true );

		MainMenu.Hide();
		SettingsMenu.Show();
		ExitButton.Show();
		State = MenuState.Settings;
	}

	public override void _Ready() {
		Control Background = GetNode<Control>( "MenuBackground" );
		Background.SetProcess( false );
		Background.SetProcessInternal( false );

		MainMenu = GetNode<Control>( "MainMenu" );
		MainMenu.Connect( "CampaignMenu", Callable.From( OnMainMenuCampaignMenu ) );
		MainMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		MainMenu.Connect( "MultiplayerMenu", Callable.From( OnMainMenuMultiplayerMenu ) );

		CampaignMenu = GetNode<Control>( "CampaignMenu" );
		CampaignMenu.SetProcess( false );
		CampaignMenu.SetProcessInternal( false );

		MultiplayerMenu = GetNode<Control>( "MultiplayerMenu" );
		MultiplayerMenu.SetProcess( false );
		MultiplayerMenu.SetProcessInternal( false );

		SettingsMenu = GetNode<Control>( "SettingsMenu" );
		SettingsMenu.SetProcess( false );
		SettingsMenu.SetProcessInternal( false );

		ExitButton = GetNode<Button>( "ExitButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			ExitButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			ExitButton.Theme = AccessibilityManager.DefaultTheme;
		}
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		LobbyBrowser = GetNode<Control>( "MultiplayerMenu/LobbyBrowser" );
		LobbyBrowser.SetProcess( false );
		LobbyBrowser.SetProcessInternal( false );

		LobbyFactory = GetNode<Control>( "MultiplayerMenu/LobbyFactory" );
		LobbyFactory.SetProcess( false );
		LobbyFactory.SetProcessInternal( false );

		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );

		AudioStreamPlayer theme = GetNode<AudioStreamPlayer>( "Theme" );
		theme.SetProcess( false );
		theme.Connect( "finished", Callable.From( OnThemeIntroFinished ) );

		LoopingTheme = ResourceLoader.Load<AudioStream>( "res://music/ui/menu_loop2.ogg" );

		GetTree().CurrentScene = this;

		SetProcess( false );
	}

	private void OnThemeIntroFinished() {
		AudioStreamPlayer theme = GetNode<AudioStreamPlayer>( "Theme" );
		theme.Stream = LoopingTheme;
		theme.Play();
		theme.Set( "parameters/looping", true );
		theme.Disconnect( "finished", Callable.From( OnThemeIntroFinished ) );
	}
}
