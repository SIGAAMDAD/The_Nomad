using Godot;
using NathanHoad;
using System;

public partial class DifficultySelect : Control {
	private Control CampaignMenu;
	private PackedScene LoadingScreen;
	private CanvasLayer NewLoadingScreen;

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

	private Label DifficultyDescription;
	private Button MemeModeButton;
	private Button IntendedExperienceButton;
	private Button PowerFantasyButton;

	private void BeginLevel( uint nDifficulty ) {
		GetNode( "/root/GameConfiguration" ).Set( "_game_difficulty", nDifficulty );

		Godot.Collections.Array<int> playerList = Input.GetConnectedJoypads();

		string levelName = "res://levels/level00";
		Hide();
		NewLoadingScreen = LoadingScreen.Instantiate<CanvasLayer>();
		GetTree().Root.AddChild( NewLoadingScreen );

		Node scene = null;
		if ( playerList.Count < 2 ) {
			// we either have someone with a controller hooked up,
			// or we're just running solo
			CommandConsole.PrintLine( "Loading level " + levelName + "_sp.tscn" );
//			GetNode( "/root/Console" ).Call( "print_line", "Loading level " + levelName + "_sp.tscn..." );
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
				levelName + "_sp.tscn"
			);
			GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );
		} else if ( playerList.Count == 2 ) {
			GetNode( "/root/Console" ).Call( "print_line", "Loading level " + levelName + "_2p.tscn..." );
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New( 
				levelName + "_2p.tscn"
			);
			GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );
		}

		scene.Connect( "OnComplete", Callable.From( OnFinishedLoading ) );
	}

	private void OnIntendedModeButtonPressed() {
		BeginLevel( 0 );
	}
	private void OnPowerFantasyModeButtonPressed() {
		BeginLevel( 1 );
	}
	private void OnIntendedModeMouseEntered() {
		DifficultyDescription.Text = "INTENDED_MODE_DESCRIPTION";
	}
	private void OnPowerFantasyModeMouseEntered() {
		DifficultyDescription.Text = "POWER_FANTASY_MODE_DESCRIPTION";
	}

	private void OnFinishedLoading() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );
		NewLoadingScreen.Hide();
		NewLoadingScreen.QueueFree();
		SoundManager.StopMusic( 1.5f );
		Hide();
	}

	public void SetMemeModeName() {
		// FIXME:?
		MemeModeButton.Text = MemeModeNameList[ new RandomNumberGenerator().RandiRange( 0, MemeModeNameList.Count - 1 ) ];
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		CampaignMenu = GetNode<Control>( "../../" );
		LoadingScreen = ResourceLoader.Load<PackedScene>( "res://scenes/menus/loading_screen.tscn" );

		DifficultyDescription = GetNode<Label>( "DifficultyDescriptionLabel" );
		MemeModeButton = GetNode<Button>( "VBoxContainer/MemeModeButton" );
		
		IntendedExperienceButton = GetNode<Button>( "VBoxContainer/IntendedModeButton" );
		IntendedExperienceButton.Connect( "pressed", Callable.From( OnIntendedModeButtonPressed ) );
		IntendedExperienceButton.Connect( "mouse_entered", Callable.From( OnIntendedModeMouseEntered ) );

		PowerFantasyButton = GetNode<Button>( "VBoxContainer/PowerFantasyModeButton" );
		PowerFantasyButton.Connect( "pressed", Callable.From( OnPowerFantasyModeButtonPressed ) );
		PowerFantasyButton.Connect( "mouse_entered", Callable.From( OnPowerFantasyModeMouseEntered ) );
	}
};