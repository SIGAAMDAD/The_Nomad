using System;
using System.Collections.Generic;
using Godot;
using Multiplayer;
using Steamworks;

public partial class LobbyBrowser : Control {
	private class LobbyData {
		private CSteamID LobbyId;
		private Button Button;
		private GameMode GameMode;

		public LobbyData( CSteamID lobbyId ) {
			LobbyId = lobbyId;

			Button = new Button();

			Refresh();
		}

		public bool Refresh() {
			if ( SteamMatchmaking.GetNumLobbyMembers( LobbyId ) == 0 ) {
				return false; // inactive
			}

			Button.Text = SteamMatchmaking.GetLobbyData( LobbyId, "name" );
			GameMode = (GameMode)Convert.ToUInt32( SteamMatchmaking.GetLobbyData( LobbyId, "gametype" ) );
			return true;
		}
		public int GetMaxMembers() {
			return SteamMatchmaking.GetLobbyMemberLimit( LobbyId );
		}
	};

	private Dictionary<CSteamID, LobbyData> LobbyList = null;

	private Button HostGame;
	private Button RefreshLobbies;
	private Button Matchmake;
	private Button CancelMatchmake;
	private AudioStreamPlayer UIChannel;

	private CSteamID SelectedLobby = CSteamID.Nil;

	private Label MapNameLabel;
	private Label PlayerCountLabel;
	private Label GameModeLabel;

	private Control MatchmakingSpinner;
	private Label MatchmakingLabel;
	private Timer MatchmakingTimer;

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

		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();
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

		string gameType = SteamMatchmaking.GetLobbyData( (CSteamID)lobbyId, "gametype" );

		Node scene = null;
		switch ( gameType ) {
		case "Multiplayer":
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
				"res://levels" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn", 1
			);
			break;
		case "Online":
			scene = (Node)ResourceLoader.Load<GDScript>( "res://addons/AsyncSceneManager/AsyncScene.gd" ).New(
				"res://levels/world.tscn"
			);
			break;
		}
		GameConfiguration.LoadedLevel = scene;
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
	private void OnLobbySelected( CSteamID lobbyId ) {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();

		SelectedLobby = lobbyId;

		PlayerCountLabel.Text = SteamMatchmaking.GetNumLobbyMembers( lobbyId ) + "/" + SteamMatchmaking.GetLobbyMemberLimit( lobbyId );

		string gameType = SteamMatchmaking.GetLobbyData( lobbyId, "gametype" );
		switch ( gameType ) {
		case "Online":
			GameModeLabel.Text = "Cooperative (LOCAL WORLD)";
			MapNameLabel.Text = "The Fever Dream";
			break;
		case "Multiplayer": {
			Mode.GameMode mode = (Mode.GameMode)Convert.ToUInt32( SteamMatchmaking.GetLobbyData( lobbyId, "gamemode" ) );
			GameModeLabel.Text = Mode.ModeNames[ mode ];

			MapNameLabel.Text = MultiplayerMapManager.MapCache[ Convert.ToInt32( SteamMatchmaking.GetLobbyData( lobbyId, "map" ) ) ].Name;
			break; }
		};
	}

	private void GetLobbyList() {
		List<CSteamID> lobbyList = SteamLobby.Instance.GetLobbyList();

		// FIXME:...?
		for ( int i = 0; i < LobbyTable.GetChildCount(); i++ ) {
			for ( int c = 0; c < LobbyTable.GetChild( i ).GetChildCount(); c++ ) {
				LobbyTable.GetChild( i ).GetChild( c ).QueueFree();
				LobbyTable.GetChild( i ).RemoveChild( LobbyTable.GetChild( i ).GetChild( c ) );
			}
			LobbyTable.GetChild( i ).QueueFree();
			LobbyTable.RemoveChild( LobbyTable.GetChild( i ) );
		}
		foreach ( var lobby in lobbyList ) {
			string lobbyName = SteamMatchmaking.GetLobbyData( lobby, "name" );

			int lobbyMemberCount = SteamMatchmaking.GetNumLobbyMembers( lobby );
			int lobbyMaxMemberCount = SteamMatchmaking.GetLobbyMemberLimit( lobby );
			string lobbyMap = SteamMatchmaking.GetLobbyData( lobby, "map" );
			string lobbyGameMode = SteamMatchmaking.GetLobbyData( lobby, "gamemode" );

			Button button = new Button();
			button.Text = lobbyName;
			button.Size = new Godot.Vector2( 240, 20 );
			button.CustomMinimumSize = button.Size;

			button.Pressed += () => { OnLobbySelected( lobby ); };

			LobbyTable.AddChild( button );
		}
	}
	private void OnRefreshButtonPressed() {
		GD.Print( "Refreshing lobbies..." );
		for ( int i = 0; i < LobbyTable.GetChildCount(); i++ ) {
			for ( int c = 0; c < LobbyTable.GetChild( i ).GetChildCount(); c++ ) {
				LobbyTable.GetChild( i ).GetChild( c ).QueueFree();
				LobbyTable.GetChild( i ).RemoveChild( LobbyTable.GetChild( i ).GetChild( c ) );
			}
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

		CancelMatchmake.Show();

		MatchmakingLabel.Text = "FINDING_MULTIPLAYER_GAME";
		MatchmakingTimer.Start();

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
		JoinButton.Connect( "pressed", Callable.From( OnJoinButtonPressed ) );

		TransitionScreen = GetNode<CanvasLayer>( "Fade" );
		TransitionScreen.Connect( "transition_finished", Callable.From( OnFinishedLoadingScene ) );

		SteamLobby.Instance.Connect( "LobbyJoined", Callable.From<ulong>( OnLobbyJoined ) );
		SteamLobby.Instance.Connect( "LobbyListUpdated", Callable.From( GetLobbyList ) );

		UIChannel = GetNode<AudioStreamPlayer>( "../../UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );
	}
};