using System.Collections.Generic;
using System.Threading;
using Godot;
using Multiplayer;
using Steamworks;

public partial class MultiplayerData : LevelData {
	private Mode ModeData = null;

	private bool Loaded = false;

	public Mode.GameMode GetMode() {
		return ModeData.GetMode();
	}

	protected override void OnResourcesFinishedLoading() {
		SetProcess( true );

		SceneLoadThread?.Join();
		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
			if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
				continue;
			}
			NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
			player.MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( SteamLobby.Instance.LobbyMembers[i] );
			player.SetOwnerId( SteamLobby.Instance.LobbyMembers[ i ] );
			ModeData.SpawnPlayer( player );
			ModeData.OnPlayerJoined( player );
			Players.Add( SteamLobby.Instance.LobbyMembers[i], player );
			PlayerList.AddChild( player );
		}

		Console.PrintLine( "...Finished loading game" );
		GetNode<LoadingScreen>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}

	protected override void OnPlayerJoined( ulong steamId ) {
		Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) ) {
			return;
		}
		
		NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
		player.MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( userId );
		player.SetOwnerId( userId );
		Players.Add( userId, player );
		PlayerList.AddChild( player );

		ModeData.OnPlayerJoined( player );
		ModeData.SpawnPlayer( player );
	}
	protected override void OnPlayerLeft( ulong steamId ) {
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
		PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" );

		ModeData = GetNode<Mode>( "ModeData" );

		ResourceLoadThread = new Thread( () => { ResourceCache.Cache( this, null ); } );
		ResourceLoadThread.Start();

		ModeData.OnPlayerJoined( ThisPlayer );
		ModeData.SpawnPlayer( ThisPlayer );

		Console.PrintLine( string.Format( "Adding {0} members...", SteamLobby.Instance.LobbyMemberCount ) );
		for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
			if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[ i ] ) || SteamLobby.Instance.LobbyMembers[ i ] == SteamUser.GetSteamID() ) {
				continue;
			}
			NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
			player.MultiplayerData = new Multiplayer.PlayerData.MultiplayerMetadata( SteamLobby.Instance.LobbyMembers[ i ] );
			player.SetOwnerId( SteamLobby.Instance.LobbyMembers[ i ] );
			ModeData.OnPlayerJoined( player );
			ModeData.SpawnPlayer( player );
			Players.Add( SteamLobby.Instance.LobbyMembers[ i ], player );
			PlayerList.AddChild( player );
		}
	}
};