using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Multiplayer;
using Steamworks;

public partial class LobbyBrowser : Control {
	private partial class LobbyData : Button {
		private CSteamID LobbyId;

		private Mode.GameMode GameMode = Mode.GameMode.Bloodbath;
		private GameMode GameType = global::GameMode.Multiplayer;
		private string MapName = "";
		private string LobbyName = "";

		public LobbyData( CSteamID lobbyId ) {
			LobbyId = lobbyId;
			Name = LobbyId.ToString();

			Size = new Godot.Vector2( 240, 20 );
			CustomMinimumSize = Size;
			Pressed += () => { OnLobbySelected( lobbyId ); };

			LoadMetadata();
		}

		public bool IsValid() {
			Refresh();

			if ( !MultiplayerMapManager.MapCache.ContainsKey( MapName ) ) {
				return false;
			}
			if ( GameMode == Mode.GameMode.Count ) {
				return false;
			}
			if ( GameType != global::GameMode.Multiplayer && GameType != global::GameMode.Online ) {
				return false;
			}
			return true;
		}
		
		private void LoadMetadata() {
			LobbyName = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
			Text = LobbyName;

			string gameMode = SteamMatchmaking.GetLobbyData( LobbyId, "gamemode" );
			if ( gameMode.IsValidInt() ) {
				GameMode = (Mode.GameMode)System.Convert.ToUInt32( gameMode );
			} else {
				GameMode = Mode.GameMode.Count;
			}
			switch ( SteamMatchmaking.GetLobbyData( LobbyId, "gametype" ) ) {
			case "Online":
				GameType = global::GameMode.Online;
				break;
			case "Multiplayer":
				GameType = global::GameMode.Multiplayer;
				break;
			default:
				break;
			};
			MapName = SteamMatchmaking.GetLobbyData( LobbyId, "map" );
		}
		public bool Refresh() {
			LoadMetadata();
			return true;
		}
		public int GetMaxMembers() {
			return SteamMatchmaking.GetLobbyMemberLimit( LobbyId );
		}
		public int GetNumMembers() {
			return SteamMatchmaking.GetNumLobbyMembers( LobbyId );
		}
		public GameMode GetGameType() {
			return GameType;
		}
		public Mode.GameMode GetGameMode() {
			return GameMode;
		}
		public string GetMapName() {
			return MapName;
		}
		public string GetMapFileName() {
			return MultiplayerMapManager.MapCache[ MapName ].FileName;
		}
		public string GetLobbyName() {
			return LobbyName;
		}
		public CSteamID GetLobbyID() {
			return LobbyId;
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

	private static CheckBox ShowFullServers;
	private static CheckBox GameFilter_LocalWorld;

	private static Timer RefreshTimer;

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

		if ( SteamLobby.Instance.IsOwner() ) {
			return;
		}

		Console.PrintLine( string.Format( "Loading game [{0}]...", SteamMatchmaking.GetLobbyData( (CSteamID)lobbyId, "gametype" ) ) );

		switch ( SteamMatchmaking.GetLobbyData( (CSteamID)lobbyId, "gametype" ) ) {
		case "Multiplayer": {
			LoadedScenePath = "res://scenes/multiplayer/lobby_room.tscn";

			// loading a multiplayer game instead a co-op world
			GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
			Console.PrintLine( "...Finished loading game" );

			GetTree().ChangeSceneToFile( "res://scenes/multiplayer/lobby_room.tscn" );
			break; }
		case "Online":
			LoadedScenePath = "res://levels/world.tscn";
			Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );

			LoadThread = new System.Threading.Thread( () => {
				LoadedWorld = ResourceLoader.Load<PackedScene>( LoadedScenePath );
				CallDeferred( "emit_signal", "FinishedLoading" );
			} );
			LoadThread.Start();
			break;
		};
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

		LobbyData data = LobbyList[ lobbyId ];
		
		// sanity check
		if ( !data.IsValid() ) {
			return;
		}

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();

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

	private bool CanShow( CSteamID lobbyId ) {
		if ( SteamMatchmaking.GetLobbyMemberLimit( lobbyId ) == SteamMatchmaking.GetNumLobbyMembers( lobbyId ) ) {
			return ShowFullServers.ButtonPressed;
		}
		return true;
	}
	private void GetLobbyList() {
		List<CSteamID> lobbyList = SteamLobby.Instance.GetLobbyList();

		for ( int i = 0; i < lobbyList.Count; i++ ) {
			for ( int j = 0; j < LobbyTable.GetChildCount(); j++ ) {
				if ( LobbyList.TryGetValue( lobbyList[i], out LobbyData lobby ) ) {
					if ( LobbyTable.FindChild( lobby.Name ) != null ) {
						LobbyTable.RemoveChild( lobby );
					}
					LobbyList.Remove( lobbyList[i] );
					lobby.QueueFree();
				}
			}
			LobbyData data = new LobbyData( lobbyList[i] );
			LobbyList.TryAdd( lobbyList[i], data );
			LobbyTable.CallDeferred( "add_child", data );
		}

		if ( MatchmakingThread.IsAlive ) {
			lock ( MatchmakingLobbyListReady ) {
				System.Threading.Monitor.Pulse( MatchmakingLobbyListReady );
			}
		}
	}
	private void OnRefreshButtonPressed() {
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

		MatchmakingLabel.Text = text;
		MatchmakingTimer.Start();
	}

	public void ResetBrowser() {
		MatchmakingSpinner.Hide();
		MatchmakingLabel.Hide();
		CancelMatchmake.Hide();

		RefreshLobbies.Show();
		HostGame.Show();
		Matchmake.Show();
	}
	
    public override void _Ready() {
		HostGame = GetNode<Button>( "ControlBar/HostButton" );
		HostGame.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		HostGame.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		HostGame.Connect( "pressed", Callable.From( OnHostGameButtonPressed ) );

		RefreshLobbies = GetNode<Button>( "ControlBar/RefreshButton" );
		RefreshLobbies.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		RefreshLobbies.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		RefreshLobbies.Connect( "pressed", Callable.From( OnRefreshButtonPressed ) );

		Matchmake = GetNode<Button>( "ControlBar/MatchmakeButton" );
		Matchmake.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		Matchmake.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		Matchmake.Connect( "pressed", Callable.From( OnMatchmakeButtonPressed ) );

		CancelMatchmake = GetNode<Button>( "ControlBar/CancelMatchmakeButton" );
		CancelMatchmake.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;
		CancelMatchmake.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MatchmakingSpinner = GetNode<Control>( "MatchMakingSpinner" );

		MatchmakingLabel = GetNode<Label>( "MatchMakingLabel" );
		MatchmakingLabel.Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		MatchmakingTimer = GetNode<Timer>( "MatchMakingLabel/MatchMakingLabelTimer" );
		MatchmakingTimer.Connect( "timeout", Callable.From( OnMatchmakingLabelTimerTimeout ) );
		
		LobbyTable = GetNode<VBoxContainer>( "LobbyList/Lobbies" );

		Label MapName = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/MapNameContainer/MapNameLabel" );
		MapNameLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/MapNameContainer/Label" );

		Label PlayerCount = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/PlayerCountContainer/PlayerCountLabel" );
		PlayerCountLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/PlayerCountContainer/Label" );

		Label GameMode = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/GameModeContainer/GameModeLabel" );
		GameModeLabel = GetNode<Label>( "LobbyMetadataContainer/VBoxContainer/GameModeContainer/Label" );

		Button JoinButton = GetNode<Button>( "ControlBar2/JoinButton" );
		JoinButton.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );
		JoinButton.Connect( "pressed", Callable.From( OnJoinButtonPressed ) );

		LobbyManager = GetNode<HBoxContainer>( "ControlBar" );
		JoinGame = GetNode<HBoxContainer>( "ControlBar2" );

		ShowFullServers = GetNode<CheckBox>( "FilterList/VBoxContainer/FullserversCheckBox" );
		ShowFullServers.Connect( "mouse_entered", Callable.From( OnButtonFocused ) );

		MatchmakingThread = new System.Threading.Thread( MatchmakingLoop );

		Timer RefreshTimer = new Timer();
		RefreshTimer.Name = "RefreshTimer";
		RefreshTimer.WaitTime = 0.5f;
		RefreshTimer.OneShot = false;
		RefreshTimer.Autostart = true;
		RefreshTimer.Connect( "timeout", Callable.From( OnRefreshButtonPressed ) );
		AddChild( RefreshTimer );

		SteamLobby.Instance.LobbyJoined += OnLobbyJoined;
		SteamLobby.Instance.Connect( "LobbyListUpdated", Callable.From( GetLobbyList ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../../../UIChannel" );

		SteamLobby.Instance.SetPhysicsProcess( true );

		Instance = this;
	}
};