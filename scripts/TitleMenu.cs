using Godot;
using NathanHoad;

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

	private MenuState State = MenuState.Main;

	private void OnExitButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		switch ( State ) {
		case MenuState.Campaign:
			CampaignMenu.Hide();
			CampaignMenu.SetProcess( false );
			break;
		case MenuState.Multiplayer:
			MultiplayerMenu.Hide();
			LobbyBrowser.Hide();
			LobbyFactory.Hide();
			LobbyBrowser.SetProcess( false );
			LobbyFactory.SetProcess( false );
			break;
		case MenuState.Settings:
			SettingsMenu.Hide();
			SettingsMenu.SetProcess( false );
			break;
		default:
			GD.PushError( "Invalid menu state!" );
			break;
		};

		MainMenu.SetProcess( true );
		ExitButton.Hide();
		MainMenu.Show();
		State = MenuState.Main;
	}
	private void OnMainMenuCampaignMenu() {
		MainMenu.SetProcess( false );
		CampaignMenu.SetProcess( true );

		MainMenu.Hide();
		CampaignMenu.Show();
		ExitButton.Show();
		State = MenuState.Campaign;
	}
	private void OnMainMenuMultiplayerMenu() {
		MainMenu.SetProcess( false );
		LobbyBrowser.SetProcess( true );

		MainMenu.Hide();
		MultiplayerMenu.Show();
		ExitButton.Show();
		LobbyBrowser.Show();
		State = MenuState.Multiplayer;
	}
	private void OnMainMenuSettingsMenu() {
		MainMenu.SetProcess( false );
		SettingsMenu.SetProcess( true );

		MainMenu.Hide();
		SettingsMenu.Show();
		ExitButton.Show();
		State = MenuState.Settings;
	}

	public override void _Ready() {
		MainMenu = GetNode<Control>( "MainMenu" );
		MainMenu.Connect( "CampaignMenu", Callable.From( OnMainMenuCampaignMenu ) );
		MainMenu.Connect( "SettingsMenu", Callable.From( OnMainMenuSettingsMenu ) );
		MainMenu.Connect( "MultiplayerMenu", Callable.From( OnMainMenuMultiplayerMenu ) );

		CampaignMenu = GetNode<Control>( "CampaignMenu" );
		CampaignMenu.SetProcess( false );

		MultiplayerMenu = GetNode<Control>( "MultiplayerMenu" );
		MultiplayerMenu.SetProcess( false );

		SettingsMenu = GetNode<Control>( "SettingsMenu" );
		SettingsMenu.SetProcess( false );

		ExitButton = GetNode<Button>( "ExitButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			ExitButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			ExitButton.Theme = AccessibilityManager.DefaultTheme;
		}
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		LobbyBrowser = GetNode<Control>( "MultiplayerMenu/LobbyBrowser" );
		LobbyBrowser.SetProcess( false );

		LobbyFactory = GetNode<Control>( "MultiplayerMenu/LobbyFactory" );
		LobbyFactory.SetProcess( false );

		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );

		SoundManager.PlayMusic( ResourceLoader.Load<AudioStream>( "res://music/ui/menu_intro.ogg" ) );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process( double delta ) {
		base._Process( delta );

		if ( ( Engine.GetProcessFrames() % 60 ) != 0 ) {
			return;
		}
		if ( !SoundManager.IsMusicPlaying() ) {
			SoundManager.PlayMusic( ResourceLoader.Load<AudioStream>( "res://music/ui/menu_loop2.ogg" ) );
		}
	}
}
