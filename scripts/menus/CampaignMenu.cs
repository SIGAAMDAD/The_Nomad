using Godot;

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
	private Button[] ButtonList = null;
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
		GameConfiguration.GameDifficulty = GameDifficulty.Intended;
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
		GameConfiguration.GameDifficulty = GameDifficulty.PowerFantasy;
	}

	private void OnMemeModeButtonPressed() {
	}

	private void OnButtonFocused( string descriptionTextID, int nButtonIndex ) {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();

		ButtonIndex = nButtonIndex;
		ButtonList[ ButtonIndex ].Modulate = Selected;
		DifficultyDescriptionLabel.Text = descriptionTextID;
	}
	private void OnButtonUnfocused() {
		ButtonList[ ButtonIndex ].Modulate = Unselected;
	}

	public void SetMemeModeName() {
		// FIXME:?
		MemeModeButton.Text = MemeModeNameList[ new RandomNumberGenerator().RandiRange( 0, MemeModeNameList.Count - 1 ) ];
	}

	public override void _ExitTree() {
		base._ExitTree();

		MemeModeNameList.Clear();

		for ( int i = 0; i < ButtonList.Length; i++ ) {
			ButtonList[i] = null;
		}
		ButtonList = null;

		QueueFree();
	}
    public override void _Ready() {
		if ( SettingsData.GetDyslexiaMode() ) {
			Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Theme = AccessibilityManager.DefaultTheme;
		}

		base._Ready();

		Label TitleLabel = GetNode<Label>( "TitleLabel" );
		TitleLabel.SetProcess( false );
		TitleLabel.SetProcessInternal( false );

		DifficultyDescriptionLabel = GetNode<Label>( "DifficultyDescriptionLabel" );
		DifficultyDescriptionLabel.SetProcess( false );
		DifficultyDescriptionLabel.SetProcessInternal( false );

		Button IntendedModeButton = GetNode<Button>( "VBoxContainer/IntendedModeButton" );
		IntendedModeButton.SetProcess( false );
		IntendedModeButton.SetProcessInternal( false );
		IntendedModeButton.Connect( "pressed", Callable.From( OnIntendedModeButtonPressed ) );
		IntendedModeButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( "INTENDED_MODE_DESCRIPTION", 0 ); } ) );
		IntendedModeButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( "INTENDED_MODE_DESCRIPTION", 0 ); } ) );
		IntendedModeButton.Connect( "mouse_exited", Callable.From( OnButtonUnfocused ) );
		IntendedModeButton.Connect( "focus_exited", Callable.From( OnButtonUnfocused ) );

		Button PowerFantasyModeButton = GetNode<Button>( "VBoxContainer/PowerFantasyModeButton" );
		PowerFantasyModeButton.SetProcess( false );
		PowerFantasyModeButton.SetProcessInternal( false );
		PowerFantasyModeButton.Connect( "pressed", Callable.From( OnPowerFantasyModeButtonPressed ) );
		PowerFantasyModeButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( "POWER_FANTASY_MODE_DESCRIPTION", 1 ); } ) );
		PowerFantasyModeButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( "POWER_FANTASY_MODE_DESCRIPTION", 1 ); } ) );
		PowerFantasyModeButton.Connect( "mouse_exited", Callable.From( OnButtonUnfocused ) );
		PowerFantasyModeButton.Connect( "focus_exited", Callable.From( OnButtonUnfocused ) );

		MemeModeButton = GetNode<Button>( "VBoxContainer/MemeModeButton" );
		MemeModeButton.SetProcess( false );
		MemeModeButton.SetProcessInternal( false );
		MemeModeButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( "MEME_MODE_DESCRIPTION", 2 ); } ) );
		MemeModeButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( "MEME_MODE_DESCRIPTION", 2 ); } ) );
		MemeModeButton.Connect( "mouse_exited", Callable.From( OnButtonUnfocused ) );
		MemeModeButton.Connect( "focus_exited", Callable.From( OnButtonUnfocused ) );

		ExitButton = GetNode<Button>( "../ExitButton" );
		UIChannel = GetNode<AudioStreamPlayer>( "../UIChannel" );
		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		
		IntendedModeButton.Modulate = Selected;
		ButtonList = [
			IntendedModeButton,
			PowerFantasyModeButton,
			MemeModeButton
		];
	}
	public override void _UnhandledInput( InputEvent @event ) {
		base._UnhandledInput( @event );

		if ( Input.IsActionJustPressed( "ui_down" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "focus_exited" );
			if ( ButtonIndex == ButtonList.Length - 1 ) {
				ButtonIndex = 0;
			} else {
				ButtonIndex++;
			}
			ButtonList[ ButtonIndex ].EmitSignal( "focus_entered" );
		}
		else if ( Input.IsActionJustPressed( "ui_up" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "focus_exited" );
			if ( ButtonIndex == 0 ) {
				ButtonIndex = ButtonList.Length - 1;
			} else {
				ButtonIndex--;
			}
			ButtonList[ ButtonIndex ].EmitSignal( "focus_entered" );
		}
		else if ( Input.IsActionJustPressed( "ui_accept" ) || Input.IsActionJustPressed( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "focus_entered" );
			ButtonList[ ButtonIndex ].CallDeferred( "emit_signal", "pressed" );
		}
	}
};
