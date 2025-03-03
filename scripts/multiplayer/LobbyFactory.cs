using Godot;
using GodotSteam;
using Multiplayer;
using NathanHoad;
using System;

public partial class LobbyFactory : Control {
	private LineEdit LobbyName;
	private HSlider MaxPlayers;
	private OptionButton MapList;
	private OptionButton GameModeList;

	private PackedScene LoadingScreen;
	private CanvasLayer NewLoadingScreen;

	public override void _Ready() {
		LobbyName = GetNode<LineEdit>( "MarginContainer/VBoxContainer/NameContainer/NameLineEdit" );
		MaxPlayers = GetNode<HSlider>( "MarginContainer/VBoxContainer/MaxPlayersContainer/MaxPlayersHSlider" );
		MapList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/MapContainer/MapOptionButton" );
		GameModeList = GetNode<OptionButton>( "MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton" );

		LoadingScreen = ResourceLoader.Load<PackedScene>( "res://scenes/menus/loading_screen.tscn" );

		Button createButton = GetNode<Button>( "MarginContainer/CreateButton" );
		createButton.Connect( "pressed", Callable.From( OnCreateButtonPressed ) );

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
		NewLoadingScreen.Hide();
		NewLoadingScreen.QueueFree();

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

		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels" + MultiplayerMapManager.MapCache[ MapList.Selected ].FileName + "_mp_" + modeName + ".tscn", 1
		);
		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );
		scene.Connect( "OnComplete", Callable.From( OnLoadedMap ) );
	}
};
