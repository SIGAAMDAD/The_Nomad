using Godot;
using Steamworks;
using Multiplayer;
using System.Threading;
using System.Collections.Generic;
using System;

public partial class LobbyRoom : Control {
	public static LobbyRoom Instance;

	private static readonly Color GoodPing = new Color( 0.0f, 1.0f, 0.0f, 1.0f );
	private static readonly Color DecentPing = new Color( 1.0f, 1.0f, 0.0f, 1.0f );
	private static readonly Color BadPing = new Color( 1.0f, 0.0f, 0.0f, 1.0f );

	private VBoxContainer PlayerList;
	private Button StartGameButton;
	private Button ExitLobbyButton;

	private HBoxContainer CurrentFocus = null;

	private readonly Color Selected = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
	private readonly Color Unselected = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

	private Thread LoadThread;
	private PackedScene LoadedLevel;

	private Label VoteLabel;

	public AudioStreamPlayer UIChannel;

	private HBoxContainer ClonerContainer;

	private Dictionary<CSteamID, bool> StartGameVotes = null;

	private readonly Color FocusColor = new Color( 0.0f, 1.0f, 0.0f, 1.0f );
	private readonly Color DefaultColor = new Color( 0.0f, 0.0f, 0.0f, 1.0f );

	[Signal]
	public delegate void FinishedLoadingEventHandler();

	public void FocusPlayer( HBoxContainer focus ) {
		if ( CurrentFocus != focus ) {
			UnfocusPlayer( CurrentFocus );
		}
		( focus.GetChild( 0 ) as Label ).Modulate = FocusColor;
		( focus.GetChild( 1 ) as Button ).Show();
		CurrentFocus = focus;
	}
	public void UnfocusPlayer( HBoxContainer focus ) {
		( focus.GetChild( 0 ) as Label ).Modulate = DefaultColor;
		( focus.GetChild( 1 ) as Button ).Hide();
	}
	public void KickPlayer( CSteamID steamId ) {
	}

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
		UIChannel.SetDeferred( "stream", UISfxManager.BeginGame );
		UIChannel.CallDeferred( "play" );

		GetNode<CanvasLayer>( "/root/SteamVoiceChat" ).Show();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).CallDeferred( "FadeOut" );

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
		}
		;

		CallDeferred( "connect", "FinishedLoading", Callable.From( OnFinishedLoading ) );
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
		container.Show();
		( container.GetChild( 3 ) as Label ).Text = SteamFriends.GetFriendPersonaName( userId );
		( container.GetChild( 4 ) as Button ).Hide();
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
			if ( ( ( PlayerList.GetChild( i ) as HBoxContainer ).GetChild( 3 ) as Label ).Text == username ) {
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
		if ( userId == SteamManager.GetSteamID() ) {
			return true;
		}
		for ( int i = 0; i < PlayerList.GetChildCount(); i++ ) {
			string username = SteamFriends.GetFriendPersonaName( userId );
			if ( ( ( PlayerList.GetChild( i ) as HBoxContainer ).GetChild( 3 ) as Label ).Text == username ) {
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

	private void PlayerKicked( CSteamID senderId ) {
	}

	private void OnVoteKickResponseYes( CSteamID senderId ) {
	}
	private void OnVoteKickResponseNo( CSteamID senderId ) {
	}

	public override void _Ready() {
		base._Ready();

		GetTree().CurrentScene = this;

		GetNode<CanvasLayer>( "/root/SteamVoiceChat" ).Hide();
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		PlayerList = GetNode<VBoxContainer>( "MarginContainer/PlayerList" );

		StartGameButton = GetNode<Button>( "StartGameButton" );
		StartGameButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( StartGameButton ); } ) );
		StartGameButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( StartGameButton ); } ) );
		StartGameButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( StartGameButton ); } ) );
		StartGameButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( StartGameButton ); } ) );
		StartGameButton.Connect( "pressed", Callable.From( OnStartGameButtonPressed ) );

		ExitLobbyButton = GetNode<Button>( "ExitLobbyButton" );
		ExitLobbyButton.Connect( "mouse_entered", Callable.From( () => { OnButtonFocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "mouse_exited", Callable.From( () => { OnButtonUnfocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "focus_entered", Callable.From( () => { OnButtonFocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "focus_exited", Callable.From( () => { OnButtonUnfocused( ExitLobbyButton ); } ) );
		ExitLobbyButton.Connect( "pressed", Callable.From( OnExitLobbyButtonPressed ) );

		VoteLabel = GetNode<Label>( "VoteLabel" );

		ClonerContainer = GetNode<HBoxContainer>( "MarginContainer/PlayerList/ClonerContainer" );

		UIChannel = GetNode<AudioStreamPlayer>( "UIChannel" );
		UIChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		//		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );

		ServerCommandManager.RegisterCommandCallback( ServerCommandType.StartGame, ( senderId ) => { LoadGame(); } );
		ServerCommandManager.RegisterCommandCallback( ServerCommandType.VoteStart, VoteStart );
		ServerCommandManager.RegisterCommandCallback( ServerCommandType.KickPlayer, PlayerKicked );
		ServerCommandManager.RegisterCommandCallback( ServerCommandType.VoteKickResponse_Yes, OnVoteKickResponseYes );
		ServerCommandManager.RegisterCommandCallback( ServerCommandType.VoteKickResponse_Yes, OnVoteKickResponseNo );
		ServerCommandManager.RegisterCommandCallback( ServerCommandType.ConnectedToLobby, ( senderId ) => {
			OnPlayerJoined( (ulong)senderId );
		} );

		HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
		container.Show();
		( container.GetChild( 3 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamManager.GetSteamID() );
		( container.GetChild( 4 ) as Button ).Hide();
		PlayerList.AddChild( container );

		SteamLobby.Instance.GetLobbyMembers();

		if ( SteamLobby.Instance.IsOwner() ) {
			StartGameVotes = new Dictionary<CSteamID, bool>( SteamLobby.MAX_LOBBY_MEMBERS );
		} else {
			for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
				if ( PlayerIsInQueue( SteamLobby.Instance.LobbyMembers[ i ] ) ) {
					continue;
				}
				container = ClonerContainer.Duplicate() as HBoxContainer;
				container.Show();
				( container.GetChild( 3 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[ i ] );
				( container.GetChild( 4 ) as Button ).Hide();
				PlayerList.AddChild( container );
			}
		}
	}
	public override void _Process( double delta ) {
		base._Process( delta );

		SteamLobby.Instance.SendP2PPacket( [ (byte)SteamLobby.MessageType.Handshake ], Constants.k_nSteamNetworkingSend_Reliable );

		SteamNetConnectionRealTimeStatus_t status = new SteamNetConnectionRealTimeStatus_t();
		SteamNetConnectionRealTimeLaneStatus_t laneStatus = new SteamNetConnectionRealTimeLaneStatus_t();

		for ( int i = 0; i < PlayerList.GetChildCount(); i++ ) {
			HBoxContainer container = PlayerList.GetChild<HBoxContainer>( i );

			{
				Label pingLabel = container.GetChild<Label>( 2 );

				if ( !SteamLobby.GetConnections().TryGetValue( SteamLobby.Instance.LobbyMembers[ i ], out HSteamNetConnection conn ) ) {
					continue;	
				}

				int ping;
				if ( SteamNetworkingSockets.GetConnectionRealTimeStatus( conn, ref status, 0, ref laneStatus ) == EResult.k_EResultOK ) {
					ping = status.m_nPing;
				} else {
					ping = int.MaxValue;
				}

				if ( ping < 60 ) {
					pingLabel.Modulate = GoodPing;
				} else if ( ping < 100 ) {
					pingLabel.Modulate = DecentPing;
				} else if ( ping >= 100 ) {
					pingLabel.Modulate = BadPing;
				}
				pingLabel.Text = string.Format( "{0}ms", ping );
			}
			{
				container.GetChild<TextureRect>( 0 ).Material.Set(
					"shader_parameter/active",
					SteamVoiceChat.Instance.IsVoiceActive( SteamLobby.Instance.LobbyMembers[ i ] )
				);
				container.GetChild<ProgressBar>( 1 ).Value = SteamVoiceChat.Instance.GetVoiceActivity(
					SteamLobby.Instance.LobbyMembers[ i ]
				);
			}
		}
	}
};
