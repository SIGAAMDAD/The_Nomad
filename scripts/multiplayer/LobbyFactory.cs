using Godot;
using Multiplayer;
using NathanHoad;
public partial class LobbyFactory : Control {
	private LineEdit LobbyName;
	private HSlider MaxPlayers;
	private OptionButton MapList;
	private OptionButton GameModeList;

	private PackedScene LoadingScreen;
	private CanvasLayer NewLoadingScreen;

	public override void _Ready() {
		Label TitleLabel = GetNode<Label>( "TitleLabel" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			TitleLabel.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			TitleLabel.Theme = AccessibilityManager.DefaultTheme;
		}

		LobbyName = GetNode<LineEdit>( "MarginContainer/VBoxContainer/NameContainer/NameLineEdit" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			LobbyName.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			LobbyName.Theme = AccessibilityManager.DefaultTheme;
		}
		MaxPlayers = GetNode<HSlider>( "MarginContainer/VBoxContainer/MaxPlayersContainer/MaxPlayersHSlider" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			MaxPlayers.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			MaxPlayers.Theme = AccessibilityManager.DefaultTheme;
		}
		MapList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/MapContainer/MapOptionButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			MapList.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			MapList.Theme = AccessibilityManager.DefaultTheme;
		}
		GameModeList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			GameModeList.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			GameModeList.Theme = AccessibilityManager.DefaultTheme;
		}

		Button CreateButton = GetNode<Button>( "MarginContainer/CreateButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			CreateButton.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			CreateButton.Theme = AccessibilityManager.DefaultTheme;
		}
		CreateButton.Connect( "pressed", Callable.From( OnCreateButtonPressed ) );

		MultiplayerMapManager.Init();

		foreach ( var map in MultiplayerMapManager.MapCache ) {
			MapList.AddItem( map.Name );
		}
		for ( int i = 0; i < (int)Mode.GameMode.Count; i++ ) {
			GameModeList.AddItem( Mode.ModeNames[ (Mode.GameMode)i ] );
		}
	}

	private void OnLoadedMap() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Hide();
		SoundManager.StopMusic( 0.5f );
		Hide();
	}
	private void OnCreateButtonPressed() {
		GetNode( "/root/Console" ).Call( "print_line", "Creating lobby...", true );

		SteamLobby.Instance.CreateLobby();
		Hide();

		SteamLobby.Instance.SetLobbyName( LobbyName.Text );
		SteamLobby.Instance.SetMaxMembers( (uint)MaxPlayers.Value );
		SteamLobby.Instance.SetMap( MapList.Selected );
		SteamLobby.Instance.SetGameMode( (uint)GameModeList.Selected );
		SteamLobby.Instance.SetHostStatus( true );
		GetNode( "/root/GameConfiguration" ).Set( "_game_mode", (uint)Player.GameMode.Multiplayer );

		NewLoadingScreen = LoadingScreen.Instantiate<CanvasLayer>();
		GetTree().Root.AddChild( NewLoadingScreen );

		string modeName;
		switch ( GameModeList.Selected ) {
		case (int)Mode.GameMode.Bloodbath:
			modeName = "bloodbath";
			break;
		case (int)Mode.GameMode.TeamBrawl:
			modeName = "teambrawl";
			break;
		case (int)Mode.GameMode.CaptureTheFlag:
			modeName = "ctf";
			break;
		default:
			return;
		};

		CommandConsole.PrintLine( "Starting game..." );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Show();

		CommandConsole.PrintLine( "Loading game..." );
//		GetNode( "/root/Console" ).Call( "print_line", "Loading level " + levelName + "_sp.tscn..." );
		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels" + MultiplayerMapManager.MapCache[ MapList.Selected ].FileName + "_mp_" + modeName + ".tscn", 1
		);
		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );
		scene.Connect( "OnComplete", Callable.From( OnLoadedMap ) );
	}
};
