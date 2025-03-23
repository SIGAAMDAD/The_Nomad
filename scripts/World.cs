using System.Threading;
using System.Collections.Generic;
using Steamworks;
using Godot;
using System.Threading.Tasks;

public partial class World : Node2D {
	[Export]
	private Player Player1 = null;
	private Node2D Hellbreaker = null;
	private Node2D SettingsData = null;

	private Control PauseMenu = null;
	private PackedScene PlayerScene = null;

	private SfxPool AudioPool;

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
	private Dictionary<CSteamID, NetworkPlayer> Players = null;	
	
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
		EmitSignal( "FinishedLoading" );
		SetProcess( true );

		SceneLoadThread.Join();
		AudioLoadThread.Join();
	}

	private void OnPlayerJoined( ulong steamId ) {
		GetNode( "/root/Console" ).Call( "print_line", "Adding " + steamId + " to game..." );

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) || userId == SteamUser.GetSteamID() ) {
			return;
		}
		
		NetworkPlayer player = PlayerScene.Instantiate<NetworkPlayer>();
		player.MultiplayerUsername = SteamFriends.GetFriendPersonaName( userId );
		player.MultiplayerId = userId;
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
		
		GetNode( "/root/Console" ).Call( "print_line", Players[ userId ].MultiplayerUsername + " has faded away..." );
		PlayerList.CallDeferred( "remove_child", Players[ userId ] );
		Players[ userId ].QueueFree();
		Players.Remove( userId );
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

//		_ = new MountainGoapLogging.DefaultLogger(
//			true
//			"goap.log"
//		);

		Players = new Dictionary<CSteamID, NetworkPlayer>();

		ThisPlayer = GetNode<Player>( "Network/Players/Player0" );
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		PlayerList = GetNode<Node>( "Network/Players" );

		PauseMenu.Connect( "leave_lobby", Callable.From( SteamLobby.Instance.LeaveLobby ) );
		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );
		
		SceneLoadThread = new Thread( () => { PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" ); } );
		SceneLoadThread.Start();

		AudioLoadThread = new Thread( () => { AudioCache.Cache( this ); } );
		AudioLoadThread.Start();

		AudioLoadingFinished += OnAudioFinishedLoading;

		SetProcess( false );
	}
};
