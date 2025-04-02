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

	private static HBoxContainer JoinGame;
	private static HBoxContainer LobbyManager;

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
	private static CheckBox GameFilter_LocalWorld;

	private static int MatchmakingPhase = 0;

	private static System.Threading.Thread MatchmakingThread = null;
	private static object MatchmakingLobbyListReady = new object();

	private System.Threading.Thread LoadThread = null;
	private PackedScene LoadedWorld = null;
	private string LoadedScenePath = "";

	public static LobbyBrowser Instance;

	[Signal]
	public delegate void OnHostGameEventHandler();
	[Signal]
	public delegate void MatchmakingFinishedEventHandler();
	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnFinishedLoading() {
		LoadThread.Join();
		
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
		Console.PrintLine( "...Finished loading game" );

		GetTree().ChangeSceneToPacked( LoadedWorld );
	}
	private void OnTransitionFinished() {
		if ( LoadedScenePath == "res://scenes/multiplayer/lobby_room.tscn" ) {
			// loading a multiplayer game instead a co-op world
			GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
			Console.PrintLine( "...Finished loading game" );

			QueueFree();
			GetTree().ChangeSceneToFile( "res://scenes/multiplayer/lobby_room.tscn" );
			return;
		}

		Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );

		LoadThread = new System.Threading.Thread( () => {
			LoadedWorld = ResourceLoader.Load<PackedScene>( LoadedScenePath );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	public void OnLobbyJoined( ulong lobbyId ) {
		/*
		if ( SettingsData.GetNetworkingEnabled() ) {
			Console.PrintLine( "Networking enabled, creating co-op lobby..." );

			GameConfiguration.GameMode = GameMode.Online;

			SteamLobby.Instance.SetMaxMembers( 4 );
			string name = SteamManager.GetSteamName();
			if ( name[ name.Length - 1 ] == 's' ) {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}' Lobby", name ) );
			} else {
				SteamLobby.Instance.SetLobbyName( string.Format( "{0}'s Lobby", name ) );
			}

			SteamLobby.Instance.CreateLobby();
		} else {
			GameConfiguration.GameMode = GameMode.SinglePlayer;
		}
		*/
		
		Console.PrintLine( string.Format( "Loading game [{0}]...", SteamMatchmaking.GetLobbyData( (CSteamID)lobbyId, "gametype" ) ) );

		switch ( SteamMatchmaking.GetLobbyData( (CSteamID)lobbyId, "gametype" ) ) {
		case "Multiplayer": {
			string modeName;
			switch ( (Mode.GameMode)Convert.ToUInt32( SteamMatchmaking.GetLobbyData( (CSteamID)lobbyId, "gamemode" ) ) ) {
			case Mode.GameMode.Bloodbath:
				modeName = "bloodbath";
				break;
			case Mode.GameMode.TeamBrawl:
				modeName = "teambrawl";
				break;
			case Mode.GameMode.CaptureTheFlag:
				modeName = "ctf";
				break;
			case Mode.GameMode.Duel:
				break;
			default:
				break;
			};
//			LoadedScenePath = "res://levels" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn";
			LoadedScenePath = "res://scenes/multiplayer/lobby_room.tscn";
			break; }
		case "Online":
			LoadedScenePath = "res://levels/world.tscn";
			break;
		};

		Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
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
		Console.PrintLine( "...no open contracts found" );
	}
	private void OnJoinGame( CSteamID lobbyId ) {
//		Tween AudioFade = GetTree().Root.CreateTween();
//		AudioFade.TweenProperty( GetTree().CurrentScene.GetNode( "Theme" ), "volume_db", -20.0f, 1.5f );
//		AudioFade.Connect( "finished", Callable.From( OnAudioFadeFinished ) );

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
		TransitionScreen.Call( "transition" );
		Hide();

		Console.PrintLine( string.Format( "Joining lobby {0}...", lobbyId.ToString() ) );
		SteamLobby.Instance.JoinLobby( lobbyId );
	}
	private static void OnLobbySelected( CSteamID lobbyId ) {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		LobbyData lobby = LobbyList[ lobbyId ];
		lobby.Refresh();

		LobbyManager.Hide();
		JoinGame.Show();

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
		GD.Print( "Building lobby list..." );

		List<CSteamID> lobbyList = SteamLobby.Instance.GetLobbyList();

		for ( int i = 0; i < lobbyList.Count; i++ ) {
			if ( LobbyList.ContainsKey( lobbyList[i] ) ) {
				// just refresh the cached data
				if ( !LobbyList[ lobbyList[i] ].Refresh() ) {
					// doesn't exist anymore
					LobbyTable.CallDeferred( "remove_child", LobbyList[ lobbyList[i] ].GetButton() );
					LobbyList.Remove( lobbyList[i] );
				}
				continue;
			}
			LobbyData data = new LobbyData( lobbyList[i] );
			LobbyList.Add( lobbyList[i], data );
			LobbyTable.CallDeferred( "add_child", data.GetButton() );
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

		LobbyManager = GetNode<HBoxContainer>( "ControlBar" );
		JoinGame = GetNode<HBoxContainer>( "ControlBar2" );

		ShowFullServers = GetNode<CheckBox>( "FilterList/VBoxContainer/FullserversCheckBox" );
		ShowFullServers.SetProcess( false );
		ShowFullServers.SetProcessInternal( false );
		ShowFullServers.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnTransitionFinished ) );

		MatchmakingThread = new System.Threading.Thread( MatchmakingLoop );

		SteamLobby.Instance.LobbyJoined += OnLobbyJoined;
		SteamLobby.Instance.Connect( "LobbyListUpdated", Callable.From( GetLobbyList ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../../UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );

		Instance = this;
	}
};