using Godot;
using NathanHoad;
using Multiplayer;
using GodotSteam;

public partial class LobbyBrowser : Control {
	private Button HostGame;
	private Button RefreshLobbies;
	private Button Matchmake;

	private Control MatchmakingSpinner;
	private Label MatchmakingLabel;

	private VBoxContainer LobbyTable;

	private PackedScene LoadingScreen;
	private CanvasLayer NewLoadingScreen;

	private int MatchmakingPhase = 0;

	[Signal]
	public delegate void OnHostGameEventHandler();

	private void OnLobbyJoined( ulong lobbyId ) {
		GetNode( "/root/Console" ).Call( "print_line", "...Joined lobby", true );

		NewLoadingScreen = (CanvasLayer)LoadingScreen.Instantiate();
		GetTree().Root.AddChild( NewLoadingScreen );

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

		GetNode( "/root/Console" ).Call( "print_line", "Starting game...", true );

		Node scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
			"res://levels" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn", 1
		);
		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );
		scene.Connect( "OnComplete", Callable.From( OnLoadedMap ) );
	}
	private void OnLoadedMap() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );
		NewLoadingScreen.Hide();
		NewLoadingScreen.QueueFree();

		SoundManager.StopMusic( 0.50f );
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
		if ( SteamLobby.Instance == null ) {
			GD.PushError( "STEAMLOBBY" );
		}
		foreach ( var lobby in SteamLobby.Instance.GetLobbyList() ) {
			string lobbyName = Steam.GetLobbyData( lobby, "name" );

			long lobbyMemberCount = Steam.GetNumLobbyMembers( lobby );
			long lobbyMaxMemberCount = Steam.GetLobbyMemberLimit( lobby );
			string lobbyMap = Steam.GetLobbyData( lobby, "map" );
			string lobbyGameMode = Steam.GetLobbyData( lobby, "gamemode" );

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

	public override void _Process( double delta ) {
		base._Process( delta );
		if ( LobbyTable.GetChildCount() == 0 ) {
			GetLobbyList();
			SetProcess( false );
		}
	}

    public override void _Ready() {
		HostGame = GetNode<Button>( "ControlBar/HostButton" );
		HostGame.Connect( "pressed", Callable.From( OnHostGameButtonPressed ) );

		RefreshLobbies = GetNode<Button>( "ControlBar/RefreshButton" );
		RefreshLobbies.Connect( "pressed", Callable.From( OnRefreshButtonPressed ) );

		Matchmake = GetNode<Button>( "ControlBar/MatchmakeButton" );

		MatchmakingSpinner = GetNode<Control>( "MatchMakingSpinner" );
		MatchmakingLabel = GetNode<Label>( "MatchMakingLabel" );
		
		LobbyTable = GetNode<VBoxContainer>( "LobbyList/Lobbies" );

		SteamLobby.Instance.Connect( "LobbyJoined", Callable.From<ulong>( OnLobbyJoined ) );
	}
};