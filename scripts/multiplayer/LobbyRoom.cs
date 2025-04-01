using Godot;
using Steamworks;
using Multiplayer;
using System.Threading;
using System.Collections.Generic;

public partial class LobbyRoom : Control {
	private VBoxContainer PlayerList;
	private Button StartGameButton;
	private Button ExitLobbyButton;

	private static Thread LoadThread;
	private static PackedScene LoadedLevel;

	private HBoxContainer ClonerContainer;

	private Dictionary<CSteamID, HBoxContainer> Players;

	[Signal]
	public delegate void FinishedLoadingEventHandler();

	private void OnFinishedLoading() {
		LoadThread.Join();
		GetTree().ChangeSceneToPacked( LoadedLevel );
		QueueFree();
	}
	private void OnStartGameButtonPressed() {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return;
		}

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
			LoadedLevel = ResourceLoader.Load<PackedScene>( "res://levels/" + MultiplayerMapManager.MapCache[ SteamLobby.Instance.GetMap() ].FileName + "_mp_" + modeName + ".tscn" );
			CallDeferred( "emit_signal", "FinishedLoading" );
		} );
		LoadThread.Start();

		SteamLobby.Instance.SendP2PPacket( CSteamID.Nil, new byte[]{ (byte)SteamLobby.MessageType.StartGame } );
	}
	private void OnExitLobbyButtonPressed() {
	}

	private void OnPlayerJoined( ulong steamId ) {
		Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) ) {
			return;
		}

		HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
		container.Show();
		( container.GetChild( 0 ) as Label ).Text = SteamFriends.GetFriendPersonaName( userId );
		PlayerList.AddChild( container );
		Players.Add( userId, container );
	}
	private void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( userId == SteamUser.GetSteamID() ) {
			return;
		}
		
		Console.PrintLine(
			string.Format( "{0} has faded away...", SteamFriends.GetFriendPersonaName( userId ) )
		);
		PlayerList.RemoveChild( Players[ userId ] );
		Players[ userId ].QueueFree();
		Players.Remove( userId );
		SteamLobby.Instance.RemovePlayer( userId );
	}

	public override void _Ready() {
		base._Ready();

		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeIn" );

		Theme = SettingsData.GetDyslexiaMode() ? AccessibilityManager.DyslexiaTheme : AccessibilityManager.DefaultTheme;

		PlayerList = GetNode<VBoxContainer>( "MarginContainer/PlayerList" );

		StartGameButton = GetNode<Button>( "StartGameButton" );
		StartGameButton.Connect( "pressed", Callable.From( OnStartGameButtonPressed ) );

		ExitLobbyButton = GetNode<Button>( "ExitLobbyButton" );
		ExitLobbyButton.Connect( "pressed", Callable.From( OnExitLobbyButtonPressed ) );

		ClonerContainer = GetNode<HBoxContainer>( "MarginContainer/PlayerList/ClonerContainer" );

		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );

		HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
		container.Show();
		( container.GetChild( 0 ) as Label ).Text = SteamManager.GetSteamName();
		PlayerList.AddChild( container );
		Players.Add( SteamManager.GetSteamID(), container );

		if ( !SteamLobby.Instance.IsOwner() ) {
			GD.Print( "Adding other players (" + SteamLobby.Instance.LobbyMembers.Count + ") to game..." );
			for ( int i = 0; i < SteamLobby.Instance.LobbyMembers.Count; i++ ) {
				if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
					continue;
				}
				container = ClonerContainer.Duplicate() as HBoxContainer;
				container.Show();
				( container.GetChild( 0 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] );
				PlayerList.AddChild( container );
				Players.Add( SteamLobby.Instance.LobbyMembers[i], container );
			}
		}
	}
};