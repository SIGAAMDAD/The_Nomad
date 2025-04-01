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

		HBoxContainer container = ClonerContainer.Duplicate() as HBoxContainer;
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

		if ( !SteamLobby.Instance.IsOwner() ) {
			GD.Print( "Adding other players (" + SteamLobby.Instance.LobbyMembers.Count + ") to game..." );
			for ( int i = 0; i < SteamLobby.Instance.LobbyMembers.Count; i++ ) {
				if ( PlayerIsInQueue( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
					continue;
				}
				container = ClonerContainer.Duplicate() as HBoxContainer;
				container.Show();
				( container.GetChild( 0 ) as Label ).Text = SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] );
				PlayerList.AddChild( container );
			}
		}
	}
};