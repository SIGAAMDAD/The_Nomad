using Godot;
using NathanHoad;
using Multiplayer;
using GodotSteam;

public partial class LobbyBrowser : Control {
	private Button HostGame;
	private Button RefreshLobbies;
	private Button Matchmake;
	private AudioStreamPlayer UIChannel;

	private Control MatchmakingSpinner;
	private Label MatchmakingLabel;

	private VBoxContainer LobbyTable;

	private int MatchmakingPhase = 0;

	[Signal]
	public delegate void OnHostGameEventHandler();

	private void OnLobbyJoined( ulong lobbyId ) {
		GetNode( "/root/Console" ).Call( "print_line", "...Joined lobby", true );

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Show();

		GetNode( "/root/Console" ).Call( "print_line", "Loading game..." );

		string modeName;
		switch ( SteamLobby.Instance.GetGameMode() ) {
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

		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn", 1
		);
		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );
		scene.Connect( "OnComplete", Callable.From( OnLoadedMap ) );
	}
	private void OnLoadedMap() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Hide();
		SoundManager.StopMusic( 0.5f );
		Hide();
	}

	private void MatchmakingLoop() {
		if ( MatchmakingPhase < 4 ) {
			Steam.AddRequestLobbyListDistanceFilter( (Steam.LobbyDistanceFilter)MatchmakingPhase );
			Steam.RequestLobbyList();
		}
		else {
			GetNode( "/root/Console" ).Call( "print_line", "No lobby found.", true );
		}
	}
	private void OnJoinGame( ulong lobbyId ) {
		GetNode( "/root/Console" ).Call( "print_line", "Joining lobby " + lobbyId.ToString() + "..." );
		SteamLobby.Instance.JoinLobby( lobbyId );
	}

	private void GetLobbyList() {
		System.Collections.Generic.List<ulong> lobbyList = SteamLobby.Instance.GetLobbyList();
		for ( int i = 0; i < lobbyList.Count; i++ ) {
			string lobbyName = Steam.GetLobbyData( lobbyList[i], "name" );

			long lobbyMemberCount = Steam.GetNumLobbyMembers( lobbyList[i] );
			long lobbyMaxMemberCount = Steam.GetLobbyMemberLimit( lobbyList[i] );
			string lobbyMap = Steam.GetLobbyData( lobbyList[i], "map" );
			string lobbyGameMode = Steam.GetLobbyData( lobbyList[i], "gamemode" );

			Button button = new Button();
			button.Text = lobbyName;
			button.Size = new Godot.Vector2( 240, 20 );

			button.Connect( "pressed", Callable.From<ulong>( OnJoinGame ) );

			LobbyTable.AddChild( button );
		}
	}
	private void OnRefreshButtonPressed() {
		foreach ( var lobby in LobbyTable.GetChildren() ) {
			LobbyTable.RemoveChild( lobby );
			lobby.QueueFree();
		}
		Steam.RequestLobbyList();
	}
	private void OnHostGameButtonPressed() {
		EmitSignal( "OnHostGame" );
	}
	private void OnMatchmakeButtonPressed() {
		MatchmakingSpinner.Show();
		MatchmakingLabel.Show();

		MatchmakingLabel.Text = "SORTING CONTRACTS...";

		MatchmakingPhase = 0;
	}
	private void OnJoinButtonPressed() {
	}

	private void OnButtonFocused() {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();
	}

	public override void _Process( double delta ) {
		base._Process( delta );
		if ( LobbyTable.GetChildCount() == 0 ) {
			GetLobbyList();
			SetProcess( false );
		}
	}
    public override void _Ready() {
		HostGame = GetNode<Button>( "ControlBar/HostButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			HostGame.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			HostGame.Theme = AccessibilityManager.DefaultTheme;
		}
		HostGame.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		HostGame.Connect( "pressed", Callable.From( OnHostGameButtonPressed ) );

		RefreshLobbies = GetNode<Button>( "ControlBar/RefreshButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			RefreshLobbies.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			RefreshLobbies.Theme = AccessibilityManager.DefaultTheme;
		}
		RefreshLobbies.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		RefreshLobbies.Connect( "pressed", Callable.From( OnRefreshButtonPressed ) );

		Matchmake = GetNode<Button>( "ControlBar/MatchmakeButton" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			Matchmake.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			Matchmake.Theme = AccessibilityManager.DefaultTheme;
		}
		Matchmake.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MatchmakingSpinner = GetNode<Control>( "MatchMakingSpinner" );
		MatchmakingLabel = GetNode<Label>( "MatchMakingLabel" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			MatchmakingLabel.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			MatchmakingLabel.Theme = AccessibilityManager.DefaultTheme;
		}
		
		LobbyTable = GetNode<VBoxContainer>( "LobbyList/Lobbies" );
		if ( (bool)GetNode( "/root/SettingsData" ).Get( "_dyslexia_mode" ) ) {
			LobbyTable.Theme = AccessibilityManager.DyslexiaTheme;
		} else {
			LobbyTable.Theme = AccessibilityManager.DefaultTheme;
		}

		SteamLobby.Instance.Connect( "LobbyJoined", Callable.From<ulong>( OnLobbyJoined ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../../UIChannel" );
	}
};