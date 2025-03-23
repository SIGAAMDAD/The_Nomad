using Godot;
using NathanHoad;
using Multiplayer;
using Steamworks;

// TODO: fix lobby refresh leaking memory

public partial class LobbyBrowser : Control {
	private Button HostGame;
	private Button RefreshLobbies;
	private Button Matchmake;
	private AudioStreamPlayer UIChannel;

	private Control MatchmakingSpinner;
	private Label MatchmakingLabel;

	private VBoxContainer LobbyTable;
	private CanvasLayer TransitionScreen;

	private int MatchmakingPhase = 0;

	[Signal]
	public delegate void OnHostGameEventHandler();

	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
	}
	private void OnFinishedLoading() {
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}
	private void OnFinishedLoadingScene() {
		( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Call( "ChangeScene" );
		QueueFree();

		Node scene = (Node)( (Node)GetNode( "/root/GameConfiguration" ).Get( "LoadedLevel" ) ).Get( "currentSceneNode" );
		scene.Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
	}

	private void OnLobbyJoined( ulong lobbyId ) {
		GetNode( "/root/Console" ).Call( "print_line", "...Joined lobby", true );

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Show();

		Tween AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		TransitionScreen.Call( "transition" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnFinishedLoadingScene ) );
		Hide();

		GetNode( "/root/LoadingScreen" ).Call( "FadeIn" );
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

//		uint gameType = Convert.ToUInt32( Steam.GetLobbyData( lobbyId, "gametype" ) );
		uint gameType = (uint)Player.GameMode.Coop2;

		Node scene;
		if ( gameType == (uint)Player.GameMode.Multiplayer ) {
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
				"res://levels" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn", 1
			);
		}
		else {
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
				"res://levels/world.tscn"
			);
		}
		GetNode( "/root/GameConfiguration" ).Set( "LoadedLevel", scene );
		scene.Connect( "OnComplete", Callable.From( OnFinishedLoadingScene ) );
	}
	
	private void MatchmakingLoop() {
		if ( MatchmakingPhase < 4 ) {
			SteamMatchmaking.AddRequestLobbyListDistanceFilter( (ELobbyDistanceFilter)MatchmakingPhase );
			SteamMatchmaking.RequestLobbyList();
		}
		else {
			GetNode( "/root/Console" ).Call( "print_line", "No lobby found.", true );
		}
	}
	private void OnJoinGame( CSteamID lobbyId ) {
		GD.Print( "Joining" );
		GetNode( "/root/Console" ).Call( "print_line", "Joining lobby " + lobbyId.ToString() + "...", true );
		SteamLobby.Instance.JoinLobby( lobbyId );
	}

	private void GetLobbyList() {
		GD.Print( "Building lobby table..." );

		System.Collections.Generic.List<CSteamID> lobbyList = SteamLobby.Instance.GetLobbyList();
		GD.Print( "Got " + lobbyList.Count + " lobbies" );
		foreach ( var lobby in lobbyList ) {
			string lobbyName = SteamMatchmaking.GetLobbyData( lobby, "name" );

			long lobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers( lobby );
			long lobbyMaxMemberCount = SteamMatchmaking.GetLobbyMemberLimit( lobby );
			string lobbyMap = SteamMatchmaking.GetLobbyData( lobby, "map" );
			string lobbyGameMode = SteamMatchmaking.GetLobbyData( lobby, "gamemode" );

			Button button = new Button();
			button.Text = lobbyName;
			button.Size = new Godot.Vector2( 240, 20 );
			button.CustomMinimumSize = button.Size;

			button.Pressed += () => { OnJoinGame( lobby ); };

			LobbyTable.AddChild( button );
		}
	}
	private void OnRefreshButtonPressed() {
		GD.Print( "Refreshing lobbies..." );
		for ( int i = 0; i < LobbyTable.GetChildCount(); i++ ) {
			LobbyTable.GetChild( i ).QueueFree();
			LobbyTable.RemoveChild( LobbyTable.GetChild( i ) );
		}

		SteamLobby.Instance.OpenLobbyList();
		GetLobbyList();
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

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );

		SteamLobby.Instance.Connect( "LobbyJoined", Callable.From<ulong>( OnLobbyJoined ) );
		SteamLobby.Instance.Connect( "LobbyListUpdated", Callable.From( GetLobbyList ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../../UIChannel" );
	}
};