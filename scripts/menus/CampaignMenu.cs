using Godot;
using NathanHoad;

public partial class CampaignMenu : Control {
	private Label DifficultyDescriptionLabel;
	private Button MemeModeButton;

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

	private void BeginLevel( uint nDifficulty ) {
		GetNode( "/root/GameConfiguration" ).Set( "_game_difficulty", nDifficulty );
		/*
		GetTree().ChangeSceneToFile( "res://scenes/menus/poem.tscn" );
		QueueFree();
		*/
		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Show();

		Node scene = null;
		CommandConsole.PrintLine( "Loading game..." );
//		GetNode( "/root/Console" ).Call( "print_line", "Loading level " + levelName + "_sp.tscn..." );
		scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://scenes/menus/poem.tscn"
		);
		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );

		scene.Connect( "OnComplete", Callable.From( OnFinishedLoading ) );
	}

	private void OnFinishedLoading() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Hide();
		SoundManager.StopMusic( 1.5f );
		Hide();
	}

	private void OnIntendedModeButtonPressed() {
		BeginLevel( 0 );
	}
	private void OnIntendedModeMouseEntered() {
		DifficultyDescriptionLabel.Text = "INTENDED_MODE_DESCRIPTION";
	}

	private void OnPowerFantasyModeButtonPressed() {
		BeginLevel( 1 );
	}
	private void OnPowerFantasyModeMouseEntered() {
		DifficultyDescriptionLabel.Text = "POWER_FANTASY_MODE_DESCRIPTION";
	}

	private void OnMemeModeButtonPressed() {
	}
	private void OnMemeModeMouseEntered() {
		DifficultyDescriptionLabel.Text = "PAIN... COMING SOON! :)";
	}

	public void SetMemeModeName() {
		// FIXME:?
		MemeModeButton.Text = MemeModeNameList[ new RandomNumberGenerator().RandiRange( 0, MemeModeNameList.Count - 1 ) ];
	}

	public override void _Ready() {
		DifficultyDescriptionLabel = GetNode<Label>( "DifficultyDescriptionLabel" );

		Button IntendedModeButton = GetNode<Button>( "VBoxContainer/IntendedModeButton" );
		IntendedModeButton.Connect( "pressed", Callable.From( OnIntendedModeButtonPressed ) );
		IntendedModeButton.Connect( "mouse_entered", Callable.From( OnIntendedModeMouseEntered ) );
		ButtonList.Add( IntendedModeButton );

		Button PowerFantasyModeButton = GetNode<Button>( "VBoxContainer/PowerFantasyModeButton" );
		PowerFantasyModeButton.Connect( "pressed", Callable.From( OnPowerFantasyModeButtonPressed ) );
		PowerFantasyModeButton.Connect( "mouse_entered", Callable.From( OnPowerFantasyModeMouseEntered ) );
		ButtonList.Add( PowerFantasyModeButton );

		MemeModeButton = GetNode<Button>( "VBoxContainer/MemeModeButton" );
		MemeModeButton.Connect( "mouse_entered", Callable.From( OnMemeModeMouseEntered ) );
		ButtonList.Add( MemeModeButton );

		ExitButton = GetNode<Button>( "../ExitButton" );
		ButtonList.Add( ExitButton );
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
		if ( Input.IsActionJustReleased( "ui_exit" ) ) {
			ExitButton.EmitSignal( "pressed" );
		}
		if ( Input.IsActionJustReleased( "ui_accept" ) || Input.IsActionJustReleased( "ui_enter" ) ) {
			ButtonList[ ButtonIndex ].EmitSignal( "pressed" );
		}
		for ( int i = 0; i < ButtonList.Count; i++ ) {
			ButtonList[ i ].Modulate = Unselected;
		}
		ButtonList[ ButtonIndex ].Modulate = Selected;
	}
};
