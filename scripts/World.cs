using System.Threading;
using System.Collections.Generic;
using Steamworks;
using Godot;

public partial class World : Node2D {
	[Export]
	private Player Player1 = null;
	private Node2D Hellbreaker = null;

	private Control PauseMenu = null;
	private PackedScene PlayerScene = null;

	[Export]
	public Node2D LevelData = null;
	[Signal]
	public delegate void FinishedLoadingEventHandler();
	[Signal]
	public delegate void AudioLoadingFinishedEventHandler();

	private bool Loaded = false;
	private Thread AudioLoadThread;
	private Thread SceneLoadThread;

	private Player ThisPlayer;
	private Dictionary<CSteamID, CharacterBody2D> Players = null;	
	
	private Node PlayerList = null;

	public void ToggleHellbreaker() {
		LevelData.Hide();
		LevelData.SetProcess( false );
		LevelData.SetProcessInput( false );
		LevelData.SetProcessInternal( false );
		LevelData.SetPhysicsProcess( false );
		LevelData.SetProcessUnhandledInput( false );

		Hellbreaker = ResourceLoader.Load<PackedScene>( "res://levels/hellbreaker" ).Instantiate<Node2D>();
		Hellbreaker.Show();
		Hellbreaker.SetProcess( true );
		Hellbreaker.SetProcessInput( true );
		Hellbreaker.SetProcessInternal( true );
		Hellbreaker.SetPhysicsProcess( true );
		Hellbreaker.SetProcessUnhandledInput( true );
		
		AddChild( Hellbreaker );
	}

	private void OnAudioFinishedLoading() {
		SetProcess( true );

		SceneLoadThread.Join();
		AudioLoadThread.Join();

		AudioCache.Initialized = true;

		if ( SettingsData.GetNetworkingEnabled() ) {
			SteamLobby.Instance.SetProcess( true );
			SteamLobby.Instance.SetPhysicsProcess( true );
		}

		if ( !SteamLobby.Instance.IsOwner() ) {
			GD.Print( "Adding other players (" + SteamLobby.Instance.LobbyMembers.Count + ") to game..." );
			for ( int i = 0; i < SteamLobby.Instance.LobbyMembers.Count; i++ ) {
				if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
					continue;
				}
				CharacterBody2D player = PlayerScene.Instantiate<CharacterBody2D>();
				player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] ) );
				player.Set( "MultiplayerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
				player.Call( "SetOwnerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
				player.GlobalPosition = new Godot.Vector2( -88720.0f, 53124.0f );
		//		SpawnPlayer( player );
				Players.Add( SteamLobby.Instance.LobbyMembers[i], player );
				PlayerList.AddChild( player );
			}
		}

		GD.Print( "Finished loading game world." );

		CallDeferred( "emit_signal", "FinishedLoading" );
	}

	private void OnPlayerJoined( ulong steamId ) {
		GetNode( "/root/Console" ).Call( "print_line", "Adding " + steamId + " to game...", true );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) ) {
			return;
		}
		
		CharacterBody2D player = PlayerScene.Instantiate<CharacterBody2D>();
		player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( userId ) );
		player.Set( "MultiplayerId", (ulong)userId );
		player.Call( "SetOwnerId", (ulong)userId );
		player.GlobalPosition = new Godot.Vector2( -88720.0f, 53124.0f );
//		SpawnPlayer( player );
		Players.Add( userId, player );
		PlayerList.AddChild( player );
	}
	private void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( userId == SteamUser.GetSteamID() ) {
			return;
		}
		
		GetNode( "/root/Console" ).Call( "print_line", (string)Players[ userId ].Get( "MultiplayerUsername" ) + " has faded away...", true );
		PlayerList.CallDeferred( "remove_child", Players[ userId ] );
		Players[ userId ].QueueFree();
		Players.Remove( userId );
		SteamLobby.Instance.RemovePlayer( userId );
	}

	public override void _ExitTree() {
		Player1.QueueFree();
		if ( Hellbreaker != null ) {
			Hellbreaker.QueueFree();
		}
	}
	public override void _Ready() {
		GetTree().CurrentScene = this;

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			Player1.SetupSplitScreen( 0 );
		}

		Players = new Dictionary<CSteamID, CharacterBody2D>();

		ThisPlayer = GetNode<Player>( "Network/Players/Player0" );
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		PlayerList = GetNode<Node>( "Network/Players" );

		PauseMenu.Connect( "LeaveLobby", Callable.From( SteamLobby.Instance.LeaveLobby ) );
		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );
		
		SceneLoadThread = new Thread( () => { PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" ); } );
		SceneLoadThread.Start();

		AudioLoadThread = new Thread( () => { AudioCache.Cache( this ); } );
		AudioLoadThread.Start();

		AudioLoadingFinished += OnAudioFinishedLoading;

		PhysicsServer2D.SetActive( true );

		SetProcess( false );
		SetProcessInternal( false );
	}
};
