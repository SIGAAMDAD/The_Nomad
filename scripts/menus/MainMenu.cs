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

	private AudioStreamPlayer UIChannel;
	private CanvasLayer TransitionScreen;

	public override void _ExitTree() {
		base._ExitTree();
	}

	private void OnFinishedLoading() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
		SoundManager.StopMusic( 1.5f );
		Hide();
	}
	private void LoadGame() {
		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		GetNode( "/root/Console" ).Call( "print_line", "Loading game..." );

		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels/world.tscn"
		);

		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );

		scene.Connect( "OnComplete", Callable.From( OnFinishedLoading ) );
	}
	private void OnContinueGameFinished() {
		LoadGame();
	}

    private void OnCampaignButtonPressed() {
		if ( ArchiveSystem.Instance.IsLoaded() ) {
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

	private void OnButtonFocused() {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();
	}

	public override void _Ready() {
		Button CampaignButton = GetNode<Button>( "VBoxContainer/CampaignButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			CampaignButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			CampaignButton.Theme = AccessibilityManager.DefaultTheme;
		}
		CampaignButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		CampaignButton.Connect( "pressed", Callable.From( OnCampaignButtonPressed ) );
		if ( ArchiveSystem.Instance.IsLoaded() ) {
			CampaignButton.Text = "CONTINUE_STORY_BUTTON";
		}
		ButtonList.Add( CampaignButton );

		Button MultiplayerButton = GetNode<Button>( "VBoxContainer/MultiplayerButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			MultiplayerButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			MultiplayerButton.Theme = AccessibilityManager.DefaultTheme;
		}
		MultiplayerButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		MultiplayerButton.Connect( "pressed", Callable.From( OnMultiplayerButtonPressed ) );
		ButtonList.Add( MultiplayerButton );

		Button SettingsButton = GetNode<Button>( "VBoxContainer/SettingsButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			SettingsButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			SettingsButton.Theme = AccessibilityManager.DefaultTheme;
		}
		SettingsButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		SettingsButton.Connect( "pressed", Callable.From( OnSettingsButtonPressed ) );
		ButtonList.Add( SettingsButton );

		Button ModsButton = GetNode<Button>( "VBoxContainer/TalesAroundTheCampfireButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			ModsButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			ModsButton.Theme = AccessibilityManager.DefaultTheme;
		}
		ModsButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		ModsButton.Connect( "pressed", Callable.From( OnModsButtonPressed ) );
		ButtonList.Add( ModsButton );

		Button ExitButton = GetNode<Button>( "VBoxContainer/QuitGameButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			ExitButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			ExitButton.Theme = AccessibilityManager.DefaultTheme;
		}
		ExitButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		ExitButton.Connect( "pressed", Callable.From( OnQuitGameButtonPressed ) );
		ButtonList.Add( ExitButton );

		Label AppVersion = GetNode<Label>( "AppVersion" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			AppVersion.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			AppVersion.Theme = AccessibilityManager.DefaultTheme;
		}
		AppVersion.Text = "App Version " + (string)ProjectSettings.GetSetting( "application/config/version" );

		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		if ( Input.IsActionJustPressed( "ui_down" ) ) {
			if ( ButtonIndex == ButtonList.Count - 1 ) {
				ButtonIndex = 0;
			} else {
				ButtonIndex++;
			}
			UIChannel.Stream = UISfxManager.ButtonFocused;
			UIChannel.Play();
		}
		if ( Input.IsActionJustPressed( "ui_up" ) ) {
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Count - 1;
			} else {
				ButtonIndex--;
			}
			UIChannel.Stream = UISfxManager.ButtonFocused;
			UIChannel.Play();
		}
		if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].CallDeferred( "emit_signal", "pressed" );
		}
		for ( int i = 0; i < ButtonList.Count; i++ ) {
			ButtonList[ i ].Modulate = Unselected;
		}
		ButtonList[ ButtonIndex ].Modulate = Selected;
	}
};
