using Godot;
using Steamworks;
using Multiplayer;
using System.Threading;
using System.Collections.Generic;

public partial class LobbyRoom : Control {
	private VBoxContainer PlayerList;
	private Button StartGameButton;
	private Button ExitLobbyButton;
	
	private readonly Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private readonly Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

	private Thread LoadThread;
	private PackedScene LoadedLevel;
	
	private AudioStreamPlayer UIChannel;

	private HBoxContainer ClonerContainer;
	
	private Dictionary<CSteamID, bool> StartGameVotes = null;

	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void VoteStart( CSteamID senderId ) {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return;
		}
		Console.PrintLine( "Received lobby vote..." );
		if ( !StartGameVotes.ContainsKey( senderId ) ) {
			StartGameVotes.Add( senderId, true );
		}
		StartGameVotes[ senderId ] = true;
	}
	private void CancelVote( CSteamID senderId ) {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return;
		}
		if ( !StartGameVotes.ContainsKey( senderId ) ) {
			StartGameVotes.Add( senderId, false );
		} else {
			StartGameVotes[ senderId ] = false;
		}
	}
	
	private void OnFinishedLoading() {
		LoadThread.Join();
		GetTree().ChangeSceneToPacked( LoadedLevel );
	}
	private void LoadGame() {
		UIChannel.Stream = UISfxManager.BeginGame;
		UIChannel.Play();

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );

		string modeName;
		switch ( (Mode.GameMode)SteamLobby.Instance.GetGameMode() ) {
		case Mode.GameMode.Bloodbath:
			modeName = "bloodbath";
			break;
		case Mode.GameMode.TeamBrawl:
			modeName = "teambrawl";
			break;
		case Mode.GameMode.CaptureTheFlag:
			modeName = "ctf";
			break;
		case Mode.GameMode.KingOfTheHill:
			modeName = "kingofthehill";
			break;
		case Mode.GameMode.Duel:
			modeName = "duel";
			break;
		default:
			return;
		};

		Connect( "FinishedLoading", Callable.From( OnFinishedLoading ) );
		LoadThread = new Thread( () => {
			LoadedLevel = ResourceLoader.Load<PackedScene>( "res://levels/" +
				MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName
				+ "_mp_" + modeName + ".tscn"
			);
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();
	}
	
	private void OnStartGameButtonPressed() {
		if ( !SteamLobby.Instance.IsOwner() ) {
			// if we're not the host, send a vote to start command
			ServerCommandManager.SendCommand( ServerCommandType.VoteStart );
			return;
		}
		
		LoadGame();
		
		ServerCommandManager.SendCommand( ServerCommandType.StartGame );
	}
	private void OnExitLobbyButtonPressed() {
		UIChannel.Stream = UISfxManager.ButtonPressed;
		UIChannel.Play();
		
		SteamLobby.Instance.LeaveLobby();
		
		Hide();
		GetTree().ChangeSceneToPacked( ResourceCache.GetScene( "res://scenes/main_menu.tscn" ) );
	}

	private void OnPlayerJoined( ulong steamId ) {
		Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;

		HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
		container.SetProcess( false );
		container.SetProcessInternal( false );
		container.Show();
		( container.GetChild( 0 ) as Label ).Text = SteamFriends.GetFriendPersonaName( userId );
		PlayerList.AddChild( container );
	}
	private void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( userId == SteamUser.GetSteamID() ) {
			return;
		}

		string username = SteamFriends.GetFriendPersonaName( userId );
		for ( int i = 0; i < PlayerList.GetChildCount(); i++ ) {
			if ( ( ( PlayerList.GetChild( i ) as HBoxContainer ).GetChild( 0 ) as Label ).Text == username ) {
				PlayerList.GetChild( i ).QueueFree();
				PlayerList.RemoveChild( PlayerList.GetChild( i ) );
				break;
			}
		}
		
		Console.PrintLine( string.Format( "{0} has faded away...", username ) );
		SteamLobby.Instance.RemovePlayer( userId );
	}

	/// <summary>
	/// if the host is currently AFK, then check to see if all the
	/// requirements are met to automatically start the game
	/// </summary>
	public void CheckAutoStart() {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return;
		}
		
		int numStartVotes = 0;
		int requiredVotes = SteamLobby.Instance.LobbyMemberCount / 2;
		 
		foreach ( var vote in StartGameVotes ) {
			if ( vote.Value ) {
				numStartVotes++;
			}
		}
		if ( numStartVotes >= requiredVotes ) {
			ServerCommandManager.SendCommand( ServerCommandType.StartGame );
		}
	}

	private bool PlayerIsInQueue( CSteamID userId ) {
		for ( int i = 0; i < PlayerList.GetChildCount(); i++ ) {
			string username = SteamFriends.GetFriendPersonaName( userId );
			if ( ( ( PlayerList.GetChild( i ) as HBoxContainer ).GetChild( 0 ) as Label ).Text == username ) {
				PlayerList.GetChild( i ).QueueFree();
				PlayerList.RemoveChild( PlayerList.GetChild( i ) );
				return true;
			}
		}
		return false;
	}
	
	private void OnButtonFocused( Button self ) {
		UIChannel.Stream = UISfxManager.ButtonFocused;
		UIChannel.Play();
		
		self.Modulate = Selected;
	}
	private void OnButtonUnfocused( Button self ) {
		self.Modulate = Unselected;
	}

	public override void _Ready() {
		base._Ready();

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );
		
		SteamLobby.Instance.SetPhysicsProcess( true );

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		PlayerList = GetNode<VBoxContainer>( "MarginContainer/PlayerList" );
		PlayerList.SetProcess( false );
		PlayerList.SetProcessInternal( false );

		StartGameButton = GetNode<Button>( "StartGameButton" );
		StartGameButton.SetProcess( false );
		StartGameButton.SetProcessInternal( false );
		StartGameButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( StartGameButton ); } ) );
		StartGameButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( StartGameButton ); } ) );
		StartGameButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( StartGameButton ); } ) );
		StartGameButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( StartGameButton ); } ) );
		StartGameButton.Connect( "pressed", Callable.From( OnStartGameButtonPressed ) );

		ExitLobbyButton = GetNode<Button>( "ExitLobbyButton" );
		ExitLobbyButton.SetProcess( false );
		ExitLobbyButton.SetProcessInternal( false );
		ExitLobbyButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "pressed", Callable.From( OnExitLobbyButtonPressed ) );

		ClonerContainer = GetNode<HBoxContainer>( "MarginContainer/PlayerList/ClonerContainer" );
		ClonerContainer.SetProcess( false );
		ClonerContainer.SetProcessInternal( false );
		
		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.SetProcess( false );
		UIChannel.SetProcessInternal( false );

		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );
		
		if ( SteamLobby.Instance.IsOwner() ) {
			StartGameVotes = new Dictionary<CSteamID, bool>( SteamLobby.MAX_LOBBY_MEMBERS );
		}
		
		ServerCommandManager.RegisterCommandCallback( ServerCommandType.StartGame, ( senderId ) => { LoadGame(); } );
		ServerCommandManager.RegisterCommandCallback( ServerCommandType.VoteStart, VoteStart );
		
		// add ourself
		HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
		container.SetProcess( false );
		container.SetProcessInternal( false );
		container.Show();
		( container.GetChild( 0 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamManager.GetSteamID() );
		PlayerList.AddChild( container );

		SteamLobby.Instance.GetLobbyMembers();

		for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
			if ( PlayerIsInQueue( SteamLobby.Instance.LobbyMembers[i] )
				|| SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() )
			{
				continue;
			}
			container = ClonerContainer.Duplicate() as HBoxContainer;
			container.SetProcess( false );
			container.SetProcessInternal( false );
			container.Show();
			( container.GetChild( 0 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] );
			PlayerList.AddChild( container );
		}
	}
};
