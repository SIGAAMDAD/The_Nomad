using Godot;
using System;
using NathanHoad;

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

	private static Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private static Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	private System.Collections.Generic.List<Button> ButtonList = new System.Collections.Generic.List<Button>();
	private int ButtonIndex = 0;

	public override void _ExitTree() {
		base._ExitTree();
	}

	private void OnFinishedLoading() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Hide();
		SoundManager.StopMusic( 1.5f );
		Hide();
	}
	private void LoadGame() {
		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Show();

		GetNode( "/root/Console" ).Call( "print_line", "Loading game..." );

		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels/world.tscn"
		);

		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );

		scene.Connect( "OnComplete", Callable.From( OnFinishedLoading ) );
	}

    private void OnCampaignButtonPressed() {
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			LoadGame();
		} else {
			EmitSignal( "CampaignMenu" );
		}
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
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			CampaignButton.Text = "CONTINUE_STORY_BUTTON";
		}
		ButtonList.Add( CampaignButton );

		Button MultiplayerButton = GetNode<Button>( "VBoxContainer/MultiplayerButton" );
		MultiplayerButton.Connect( "pressed", Callable.From( OnMultiplayerButtonPressed ) );
		ButtonList.Add( MultiplayerButton );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		SettingsButton.Connect( "pressed", Callable.From( OnSettingsButtonPressed ) );
		ButtonList.Add( SettingsButton );

		Button ModsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		ModsButton.Connect( "pressed", Callable.From( OnModsButtonPressed ) );
		ButtonList.Add( ModsButton );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		ExitButton.Connect( "pressed", Callable.From( OnQuitGameButtonPressed ) );
		ButtonList.Add( ExitButton );

		Label AppVersion = GetNode<Label>( "AppVersion" );
		AppVersion.Text = "AppVer " + (string)ProjectSettings.GetSetting( "application/config/version" );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		if ( Input.IsActionJustPressed( "ui_down" ) ) {
			if ( ButtonIndex == ButtonList.Count - 1 ) {
				ButtonIndex = 0;
			} else {
				ButtonIndex++;
			}
		}
		if ( Input.IsActionJustPressed( "ui_up" ) ) {
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Count - 1;
			} else {
				ButtonIndex--;
			}
		}
		if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "pressed" );
		}
		for ( int i = 0; i < ButtonList.Count; i++ ) {
			ButtonList[ i ].Modulate = Unselected;
		}
		ButtonList[ ButtonIndex ].Modulate = Selected;
	}
};
