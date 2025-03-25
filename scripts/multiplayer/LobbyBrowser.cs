using System;
using System.Collections.Generic;
using Godot;
using Multiplayer;
using Steamworks;

public partial class LobbyBrowser : Control {
	private class LobbyData {
		private CSteamID LobbyId;
		private Button Button;

		public LobbyData( CSteamID lobbyId ) {
			LobbyId = lobbyId;

			Button = new Button();
			Button.Size = new Godot.Vector2( 240, 20 );
			Button.CustomMinimumSize = Button.Size;
			Button.Pressed += () => { OnLobbySelected( lobbyId ); };

			Refresh();
		}

		public bool Refresh() {
			if ( SteamMatchmaking.GetNumLobbyMembers( LobbyId ) == 0 ) {
				return false; // inactive
			}

			Button.Text = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
			return true;
		}
		public int GetMaxMembers() {
			return SteamMatchmaking.GetLobbyMemberLimit( LobbyId );
		}
		public int GetNumMembers() {
			return SteamMatchmaking.GetNumLobbyMembers( LobbyId );
		}
		public GameMode GetGameType() {
			string gameType = SteamMatchmaking.GetLobbyData( LobbyId, "gametype" );
			switch ( gameType ) {
			case "Online":
				return GameMode.Online;
			case "Multiplayer":
				return GameMode.Multiplayer;
			default:
				break;
			};
			GD.PushError( "[STEAM] Lobby " + LobbyId.ToString() + " doens't have a valid gametype: " + gameType );
			return GameMode.SinglePlayer;
		}
		public Mode.GameMode GetGameMode() {
			return (Mode.GameMode)Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" ) );
		}
		public string GetMapName() {
			return MultiplayerMapManager.MapCache[ Convert.ToInt32( SteamMatchmaking.GetLobbyData( LobbyId, "map" ) ) ].Name;
		}
		public string GetMapFileName() {
			return MultiplayerMapManager.MapCache[ Convert.ToInt32( SteamMatchmaking.GetLobbyData( LobbyId, "map" ) ) ].FileName;
		}
		public string GetName() {
			return SteamMatchmaking.GetLobbyData( LobbyId, "name" );
		}
		public Button GetButton() {
			return Button;
		}
	};

	private static Dictionary<CSteamID, LobbyData> LobbyList = new Dictionary<CSteamID, LobbyData>();
	private static Dictionary<int, bool> MapFilterList = new Dictionary<int, bool>();

	private static Button HostGame;
	private static Button RefreshLobbies;
	private static Button Matchmake;
	private static Button CancelMatchmake;
	private static AudioStreamPlayer UIChannel;

	private static CSteamID SelectedLobby = CSteamID.Nil;

	private static Label MapNameLabel;
	private static Label PlayerCountLabel;
	private static Label GameModeLabel;

	private static Control MatchmakingSpinner;
	private static Label MatchmakingLabel;
	private static Timer MatchmakingTimer;

	private static VBoxContainer LobbyTable;
	private static CanvasLayer TransitionScreen;

	private static CheckBox ShowFullServers;
	private static ItemList MapFilters;
	private static ItemList GameModeFilters;

	private static int MatchmakingPhase = 0;

	private static System.Threading.Thread MatchmakingThread = null;
	private static object MatchmakingLobbyListReady = new object();

	[Signal]
	public delegate void OnHostGameEventHandler();
	[Signal]
	public delegate void MatchmakingFinishedEventHandler();

	private void OnAudioFadeFinished() {
		GetTree().CurrentScene.GetNode<AudioStreamPlayer>( "Theme" ).Stop();
	}
	private void OnFinishedLoading() {
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}
	private void OnFinishedLoadingScene() {
		GameConfiguration.LoadedLevel.Call( "ChangeScene" );

		Node scene = (Node)GameConfiguration.LoadedLevel.Get( "currentSceneNode" );
		scene.Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
	}

	private void OnLobbyJoined( ulong lobbyId ) {
		GetNode( "/root/Console" ).Call( "print_line", "...Joined lobby", true );

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Show();

		Tween AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		LobbyData lobby = LobbyList[ (CSteamID)lobbyId ];
		lobby.Refresh();

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		Hide();

		GetNode( "/root/LoadingScreen" ).Call( "FadeIn" );
		GetNode( "/root/Console" ).Call( "print_line", "Loading game..." );

		Node scene = null;
		switch ( lobby.GetGameType() ) {
		case GameMode.Multiplayer: {
			string modeName;
			switch ( lobby.GetGameMode() ) {
			case Mode.GameMode.Bloodbath:
				modeName = "bloodbath";
				break;
			case Mode.GameMode.TeamBrawl:
				modeName = "teambrawl";
				break;
			case Mode.GameMode.CaptureTheFlag:
				modeName = "ctf";
				break;
			default:
				return;
			};
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
				"res://levels" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn", 1
			);
			break; }
		case GameMode.Online:
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
				"res://levels/world.tscn"
			);
			break;
		}
		GameConfiguration.LoadedLevel = scene;
		scene.Connect( "OnComplete", Callable.From( OnFinishedLoadingScene ) );
	}

	private void MatchmakingLoop() {
		// apply filters
//		SteamMatchmaking.AddRequestLobbyListStringFilter( "map" )

		while ( MatchmakingPhase < 4 ) {
			lock ( MatchmakingLobbyListReady ) {
				System.Threading.Monitor.Wait( MatchmakingLobbyListReady );
			}

			SteamMatchmaking.AddRequestLobbyListDistanceFilter( (ELobbyDistanceFilter)MatchmakingPhase );
			SteamMatchmaking.RequestLobbyList();
		}
		GetNode( "/root/Console" ).Call( "print_line", "...No open contracts found", true );
	}
	private void OnJoinGame( CSteamID lobbyId ) {
		Tween AudioFade = GetTree().Root.CreateTween();
		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		TransitionScreen.Call( "transition" );
		Hide();

		GetNode( "/root/Console" ).Call( "print_line", "Joining lobby " + lobbyId.ToString() + "...", true );
		SteamLobby.Instance.JoinLobby( lobbyId );
	}
	private static void OnLobbySelected( CSteamID lobbyId ) {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		LobbyData lobby = LobbyList[ lobbyId ];
		lobby.Refresh();

		SelectedLobby = lobbyId;
		PlayerCountLabel.Text = lobby.GetNumMembers() + "/" + lobby.GetMaxMembers();

		switch ( lobby.GetGameType() ) {
		case GameMode.Online:
			GameModeLabel.Text = "Cooperative (LOCAL WORLD)";
			MapNameLabel.Text = "The Fever Dream";
			break;
		case GameMode.Multiplayer: {
			Mode.GameMode mode = lobby.GetGameMode();
			GameModeLabel.Text = Mode.ModeNames[ mode ];

			MapNameLabel.Text = lobby.GetMapName();
			break; }
		};
	}

	private void GetLobbyList() {
		List<CSteamID> lobbyList = SteamLobby.Instance.GetLobbyList();

		for ( int i = 0; i < lobbyList.Count; i++ ) {
			if ( LobbyList.ContainsKey( lobbyList[i] ) ) {
				// just refresh the cached data
				if ( !LobbyList[ lobbyList[i] ].Refresh() ) {
					// doesn't exist anymore
					LobbyTable.RemoveChild( LobbyList[ lobbyList[i] ].GetButton() );
					LobbyList.Remove( lobbyList[i] );
				}
				continue;
			}
			LobbyData data = new LobbyData( lobbyList[i] );
			LobbyList.Add( lobbyList[i], data );
			LobbyTable.AddChild( data.GetButton() );
		}

		if ( MatchmakingThread.IsAlive ) {
			lock ( MatchmakingLobbyListReady ) {
				System.Threading.Monitor.Pulse( MatchmakingLobbyListReady );
			}
		}
	}
	private void OnRefreshButtonPressed() {
		GD.Print( "Refreshing lobbies..." );
		SteamLobby.Instance.OpenLobbyList();
		GetLobbyList();
	}
	private void OnHostGameButtonPressed() {
		EmitSignal( "OnHostGame" );
	}
	private void OnMatchmakeButtonPressed() {
		MatchmakingSpinner.Show();
		MatchmakingLabel.Show();

		CancelMatchmake.Show();

		MatchmakingLabel.Text = "FINDING_MULTIPLAYER_GAME";
		MatchmakingTimer.Start();

		MatchmakingThread.Start();

		MatchmakingPhase = 0;
	}
	private void OnJoinButtonPressed() {
		if ( MatchmakingLabel.Visible ) {
			return; // matchmaking, can't join game
		}

		OnJoinGame( SelectedLobby );
	}

	private void OnButtonFocused() {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();
	}

	private void OnMatchmakingLabelTimerTimeout() {
		string text = MatchmakingLabel.Text;

		int position = 0;
		int numDots = 0;
		while ( text.Find( '.', position, false ) != -1 ) {
			numDots++;
		}

		if ( numDots < 3 ) {
			text += ".";
		} else {
			text = "FINDING_MULTIPLAYER_GAME";
		}

		MatchmakingTimer.Start();
	}

    public override void _Ready() {
		HostGame = GetNode<Button>( "ControlBar/HostButton" );
		HostGame.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		HostGame.SetProcess( false );
		HostGame.SetProcessInternal( false );
		HostGame.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		HostGame.Connect( "pressed", Callable.From( OnHostGameButtonPressed ) );

		RefreshLobbies = GetNode<Button>( "ControlBar/RefreshButton" );
		RefreshLobbies.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		RefreshLobbies.SetProcess( false );
		RefreshLobbies.SetProcessInternal( false );
		RefreshLobbies.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		RefreshLobbies.Connect( "pressed", Callable.From( OnRefreshButtonPressed ) );

		Matchmake = GetNode<Button>( "ControlBar/MatchmakeButton" );
		Matchmake.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		Matchmake.SetProcess( false );
		Matchmake.SetProcessInternal( false );
		Matchmake.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		Matchmake.Connect( "pressed", Callable.From( OnMatchmakeButtonPressed ) );

		CancelMatchmake = GetNode<Button>( "ControlBar/CancelMatchmakeButton" );
		CancelMatchmake.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		CancelMatchmake.SetProcess( false );
		CancelMatchmake.SetProcessInternal( false );
		CancelMatchmake.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MatchmakingSpinner = GetNode<Control>( "MatchMakingSpinner" );

		MatchmakingLabel = GetNode<Label>( "MatchMakingLabel" );
		MatchmakingLabel.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		MatchmakingLabel.SetProcess( false );
		MatchmakingLabel.SetProcessInternal( false );

		MatchmakingTimer = GetNode<Timer>( "MatchMakingLabel/MatchMakingLabelTimer" );
		MatchmakingTimer.Connect( "timeout", Callable.From( OnMatchmakingLabelTimerTimeout ) );
		
		LobbyTable = GetNode<VBoxContainer>( "LobbyList/Lobbies" );
		LobbyTable.SetProcess( false );
		LobbyTable.SetProcessInternal( false );

		Label MapName = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/MapNameContainer/MapNameLabel" );
		MapName.SetProcess( false );
		MapName.SetProcessInternal( false );

		MapNameLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/MapNameContainer/Label" );
		MapNameLabel.SetProcess( false );
		MapNameLabel.SetProcessInternal( false );

		Label PlayerCount = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/PlayerCountContainer/PlayerCountLabel" );
		PlayerCount.SetProcess( false );
		PlayerCount.SetProcessInternal( false );

		PlayerCountLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/PlayerCountContainer/Label" );
		PlayerCountLabel.SetProcess( false );
		PlayerCountLabel.SetProcessInternal( false );

		Label GameMode = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/GameModeContainer/GameModeLabel" );
		GameMode.SetProcess( false );
		GameMode.SetProcessInternal( false );

		GameModeLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/GameModeContainer/Label" );
		GameModeLabel.SetProcess( false );
		GameModeLabel.SetProcessInternal( false );

		Button JoinButton = GetNode<Button>( "ControlBar2/JoinButton" );
		JoinButton.SetProcess( false );
		JoinButton.SetProcessInternal( false );
		JoinButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		JoinButton.Connect( "pressed", Callable.From( OnJoinButtonPressed ) );

		ShowFullServers = GetNode<CheckBox>( "FilterList/VBoxContainer/FullserversCheckBox" );
		ShowFullServers.SetProcess( false );
		ShowFullServers.SetProcessInternal( false );
		ShowFullServers.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		GameModeFilters = GetNode<ItemList>( "FilterList/VBoxContainer/GameModeItemList" );
		GameModeFilters.SetProcess( false );
		GameModeFilters.SetProcessInternal( false );

		MapFilters = GetNode<ItemList>( "FilterList/VBoxContainer/MapItemList" );
		MapFilters.SetProcess( false );
		MapFilters.SetProcessInternal( false );

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnFinishedLoadingScene ) );

		MatchmakingThread = new System.Threading.Thread( MatchmakingLoop );

		SteamLobby.Instance.Connect( "LobbyJoined", Callable.From<ulong>( OnLobbyJoined ) );
		SteamLobby.Instance.Connect( "LobbyListUpdated", Callable.From( GetLobbyList ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../../UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );
	}
};