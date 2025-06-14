using System.Collections.Generic;
using System.Threading;
using Godot;
using Multiplayer;
using Steamworks;

public partial class MultiplayerData : Node2D {
	private Control PauseMenu = null;
	private PackedScene PlayerScene = null;

	private Mode ModeData = null;

	private bool Loaded = false;
	private Thread ResourceLoadThread = null;
	private Thread SceneLoadThread = null;

	private Player ThisPlayer = null;
	private Dictionary<CSteamID, Renown.Entity> Players = null;
	private Node PlayerList = null;

	[Signal]
	public delegate void ResourcesLoadingFinishedEventHandler();

	public Mode.GameMode GetMode() {
		return ModeData.GetMode();
	}

	private void OnResourcesFinishedLoading() {
		SetProcess( true );

		SceneLoadThread.Join();
		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		ModeData.OnPlayerJoined( ThisPlayer );
		ModeData.SpawnPlayer( ThisPlayer );

		for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
			if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
				continue;
			}
			NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
			player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] ) );
			player.Set( "MultiplayerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
			player.Call( "SetOwnerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
			ModeData.SpawnPlayer( player );
			Players.Add( SteamLobby.Instance.LobbyMembers[i], player );
			PlayerList.AddChild( player );
		}

		Console.PrintLine( "...Finished loading game" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}

	private void OnPlayerJoined( ulong steamId ) {
		Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) ) {
			return;
		}
		
		NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
		player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( userId ) );
		player.Set( "MultiplayerId", (ulong)userId );
		player.Call( "SetOwnerId", (ulong)userId );
		Players.Add( userId, player );
		PlayerList.AddChild( player );

		ModeData.OnPlayerJoined( player );
		ModeData.SpawnPlayer( player );
	}
	private void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( userId == SteamUser.GetSteamID() ) {
			return;
		}

		ModeData.OnPlayerLeft( Players[ userId ] );
		
		Console.PrintLine(
			string.Format( "{0} has faded away...", ( Players[ userId ] as NetworkPlayer ).MultiplayerData.Username )
		);
		PlayerList.CallDeferred( "remove_child", Players[ userId ] );
		Players[ userId ].QueueFree();
		Players.Remove( userId );
		SteamLobby.Instance.RemovePlayer( userId );
	}
	
	public override void _Ready() {
		base._Ready();

		Players = new Dictionary<CSteamID, Renown.Entity>();

		SceneLoadThread = new Thread( () => {
			PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" );
		} );

		ModeData = GetNode<Mode>( "ModeData" );

		ResourceLoadThread = new Thread( () => { ResourceCache.Cache( this, SceneLoadThread ); } );
		ResourceLoadThread.Start();

		ModeData.OnPlayerJoined( ThisPlayer );
		ModeData.SpawnPlayer( ThisPlayer );

		Console.PrintLine( string.Format( "Adding {0} members...", SteamLobby.Instance.LobbyMemberCount ) );
		for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
			if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
				continue;
			}
			Renown.Entity player = PlayerScene.Instantiate<NetworkPlayer>();
			player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] ) );
			player.Set( "MultiplayerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
			player.Call( "SetOwnerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
			ModeData.SpawnPlayer( player );
			Players.Add( SteamLobby.Instance.LobbyMembers[i], player );
			PlayerList.AddChild( player );
		}
	}
};