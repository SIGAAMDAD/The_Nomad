using Godot;
using NathanHoad;
using System;

public partial class TitleMenu : Control {
	public enum MenuState {
		Main,
		Campaign,
		Multiplayer,
		Settings,
		Help,
		Mods
	};

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
		switch ( State ) {
		case MenuState.Campaign:
			CampaignMenu.Hide();
			SaveSlotSelect.Hide();
			DifficultySelect.Hide();
			break;
		case MenuState.Multiplayer:
			MultiplayerMenu.Hide();
			LobbyBrowser.Hide();
			LobbyFactory.Hide();
			break;
		case MenuState.Settings:
			SettingsMenu.Hide();
			break;
		default:
			GD.PushError( "Invalid menu state!" );
			break;
		};

		ExitButton.Hide();
		MainMenu.Show();
		State = MenuState.Main;
	}
	private void OnMainMenuCampaignMenu() {
		MainMenu.Hide();
		CampaignMenu.Show();
		SaveSlotSelect.Show();
		ExitButton.Show();
		State = MenuState.Campaign;
	}
	private void OnMainMenuMultiplayerMenu() {
		MainMenu.Hide();
		MultiplayerMenu.Show();
		ExitButton.Show();
		LobbyBrowser.Show();
		State = MenuState.Multiplayer;
	}
	private void OnMainMenuSettingsMenu() {
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
		MultiplayerMenu = GetNode<Control>( "MultiplayerMenu" );
		SettingsMenu = GetNode<Control>( "SettingsMenu" );

		ExitButton = GetNode<Button>( "ExitButton" );
		ExitButton.Connect( "pressed", Callable.From( OnExitButtonPressed ) );

		SaveSlotSelect = GetNode<Control>( "CampaignMenu/SaveSlotSelect" );
		DifficultySelect = GetNode<Control>( "CampaignMenu/DifficultySelect" );

		LobbyBrowser = GetNode<Control>( "MultiplayerMenu/LobbyBrowser" );
		LobbyFactory = GetNode<Control>( "MultiplayerMenu/LobbyFactory" );

		SoundManager.PlayMusic( ResourceLoader.Load<AudioStream>( "res://music/ui/main.ogg" ) );
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
