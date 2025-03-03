using Godot;
using System;

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

	private void OnCampaignButtonPressed() {
		EmitSignal( "CampaignMenu" );
	}
	private void OnSettingsButtonPressed() {
		EmitSignal( "SettingsMenu" );
	}
	private void OnModsButtonPressed() {
		EmitSignal( "ModsMenu" );
	}
	private void OnMultiplayerButtonPressed() {
		EmitSignal( "MultiplayerMenu" );
	}
	private void OnQuitGameButtonPressed() {
		GetTree().Quit();
	}

	public override void _Ready() {
		Button CampaignButton = GetNode<Button>( "VBoxContainer/CampaignButton" );
		CampaignButton.Connect( "pressed", Callable.From( OnCampaignButtonPressed ) );

		Button MultiplayerButton = GetNode<Button>( "VBoxContainer/MultiplayerButton" );
		MultiplayerButton.Connect( "pressed", Callable.From( OnMultiplayerButtonPressed ) );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.Connect( "pressed", Callable.From( OnSettingsButtonPressed ) );

//		Button ModsButton = GetNode<Button>( "VBoxContainer/ModsButton" );
//		ModsButton.Connect( "pressed", Callable.From( OnModsButtonPressed ) );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		ExitButton.Connect( "pressed", Callable.From( OnQuitGameButtonPressed ) );
	}
};
