using System.ComponentModel.Design.Serialization;
using Godot;
using NathanHoad;

public partial class CampaignMenu : Control {
	private Label DifficultyDescriptionLabel;
	private Button MemeModeButton;
	private AudioStreamPlayer UIChannel;

	private System.Collections.Generic.List<string> MemeModeNameList = new System.Collections.Generic.List<string>{
		"POV: Kazuma",
		"Dark Souls",
		"Writing in C++",
		"Metal Goose Rising: REVENGEANCE",
		"Hell Itself",
		"Suicidal Encouragement",
		"Cope & Seethe, Repeat",
		"Sounds LIke a U Problem",
		"GIT GUD",
		"THE MEMES",
		"Deal With It",
		"Just A Minor Inconvenience",
		"YOU vs God",
		"The Ultimate Bitch-Slap",
		"GIT REKT",
		"GET PWNED",
		"Wish U Had A BFG?",
		"Skill Issue",
		"DAKKA",
		"OOOOF",
		"So sad, Too bad",
		"Actual Living Hell",
		"RNJesus Hates You",
		"I AM THE DANGER",
		"Awwww Does This Make U Cry?",
		"Asian"
	};

	private static Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private static Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	private System.Collections.Generic.List<Button> ButtonList = new System.Collections.Generic.List<Button>();
	private int ButtonIndex = 0;
	private Button ExitButton;
	private CanvasLayer TransitionScreen;
	private bool Loading = false;

	private void BeginLevel() {
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
		QueueFree();
	}

	private void OnBeginGameFinished() {
		BeginLevel();
	}
	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
	}

	private void OnIntendedModeButtonPressed() {
		if ( Loading ) {
			return;
		}
		Tween AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		Loading = true;
		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		TransitionScreen.Call( "transition" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		GetNode( "/root/GameConfiguration" ).Set( "_game_difficulty", 0 );
	}
	private void OnIntendedModeMouseEntered() {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();

		DifficultyDescriptionLabel.Text = "INTENDED_MODE_DESCRIPTION";
	}

	private void OnPowerFantasyModeButtonPressed() {
		if ( Loading ) {
			return;
		}
		Tween AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		Loading = true;
		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		TransitionScreen.Call( "transition" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnBeginGameFinished ) );
		GetNode( "/root/GameConfiguration" ).Set( "_game_difficulty", 1 );
	}
	private void OnPowerFantasyModeMouseEntered() {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();

		DifficultyDescriptionLabel.Text = "POWER_FANTASY_MODE_DESCRIPTION";
	}

	private void OnMemeModeButtonPressed() {
	}
	private void OnMemeModeMouseEntered() {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();

		DifficultyDescriptionLabel.Text = "PAIN... COMING SOON! :)";
	}

	public void SetMemeModeName() {
		// FIXME:?
		MemeModeButton.Text = MemeModeNameList[ new RandomNumberGenerator().RandiRange( 0, MemeModeNameList.Count - 1 ) ];
	}

	public override void _Ready() {
		Label TitleLabel = GetNode<Label>( "TitleLabel" );
		if ( SettingsData.GetDyslexiaMode() ) {
			TitleLabel.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			TitleLabel.Theme = AccessibilityManager.DefaultTheme;
		}

		DifficultyDescriptionLabel = GetNode<Label>( "DifficultyDescriptionLabel" );
		if ( SettingsData.GetDyslexiaMode() ) {
			DifficultyDescriptionLabel.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			DifficultyDescriptionLabel.Theme = AccessibilityManager.DefaultTheme;
		}

		Button IntendedModeButton = GetNode<Button>( "VBoxContainer/IntendedModeButton" );
		if ( SettingsData.GetDyslexiaMode() ) {
			IntendedModeButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			IntendedModeButton.Theme = AccessibilityManager.DefaultTheme;
		}
		IntendedModeButton.Connect( "pressed", Callable.From( OnIntendedModeButtonPressed ) );
		IntendedModeButton.Connect( "mouse_entered", Callable.From( OnIntendedModeMouseEntered ) );
		ButtonList.Add( IntendedModeButton );

		Button PowerFantasyModeButton = GetNode<Button>( "VBoxContainer/PowerFantasyModeButton" );
		if ( SettingsData.GetDyslexiaMode() ) {
			PowerFantasyModeButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			PowerFantasyModeButton.Theme = AccessibilityManager.DefaultTheme;
		}
		PowerFantasyModeButton.Connect( "pressed", Callable.From( OnPowerFantasyModeButtonPressed ) );
		PowerFantasyModeButton.Connect( "mouse_entered", Callable.From( OnPowerFantasyModeMouseEntered ) );
		ButtonList.Add( PowerFantasyModeButton );

		MemeModeButton = GetNode<Button>( "VBoxContainer/MemeModeButton" );
		if ( SettingsData.GetDyslexiaMode() ) {
			MemeModeButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			MemeModeButton.Theme = AccessibilityManager.DefaultTheme;
		}
		MemeModeButton.Connect( "mouse_entered", Callable.From( OnMemeModeMouseEntered ) );
		ButtonList.Add( MemeModeButton );

		ExitButton = GetNode<Button>( "../ExitButton" );
		ButtonList.Add( ExitButton );

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
			switch ( ButtonIndex ) {
			case 0:
				OnIntendedModeMouseEntered();
				break;
			case 1:
				OnPowerFantasyModeMouseEntered();
				break;
			case 2:
				OnMemeModeMouseEntered();
				break;
			};
			UIChannel.Stream = UISfxManager.ButtonFocused;
			UIChannel.Play();
		}
		if ( Input.IsActionJustPressed( "ui_up" ) ) {
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Count - 1;
			} else {
				ButtonIndex--;
			}
			switch ( ButtonIndex ) {
			case 0:
				OnIntendedModeMouseEntered();
				break;
			case 1:
				OnPowerFantasyModeMouseEntered();
				break;
			case 2:
				OnMemeModeMouseEntered();
				break;
			};
			UIChannel.Stream = UISfxManager.ButtonFocused;
			UIChannel.Play();
		}
		if ( Input.IsActionJustPressed( "ui_exit" ) ) {
			ExitButton.EmitSignal( "pressed" );
			UIChannel.Stream = UISfxManager.ButtonPressed;
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
