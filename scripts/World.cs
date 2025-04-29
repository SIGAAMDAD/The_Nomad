using System.Threading;
using System.Collections.Generic;
using Steamworks;
using Godot;
using Renown.World;
using Renown;
using System.Diagnostics;
using System;

public partial class World : Node2D {
	private Node2D Hellbreaker = null;

	private PauseMenu PauseMenu = null;
	private PackedScene PlayerScene = null;

	[Export]
	public Node2D LevelData = null;

	private Thread ResourceLoadThread;
	private Thread SceneLoadThread;

	private Player ThisPlayer;
	private Dictionary<CSteamID, Renown.Entity> Players = null;	
	private Node PlayerList = null;

	[Signal]
	public delegate void ResourcesLoadingFinishedEventHandler();
	[Signal]
	public delegate void RenownInitFinishedEventHandler();

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

	private void OnResourcesFinishedLoading() {
		SetProcess( true );

		SceneLoadThread.Join();
		ResourceLoadThread.Join();

		ResourceCache.Initialized = true;

		if ( SettingsData.GetNetworkingEnabled() ) {
			SteamLobby.Instance.SetProcess( true );
			SteamLobby.Instance.SetPhysicsProcess( true );
		}

		if ( !SteamLobby.Instance.IsOwner() ) {
			GD.Print( "Adding other players (" + SteamLobby.Instance.LobbyMemberCount + ") to game..." );
			for ( int i = 0; i < SteamLobby.Instance.LobbyMemberCount; i++ ) {
				if ( Players.ContainsKey( SteamLobby.Instance.LobbyMembers[i] ) || SteamLobby.Instance.LobbyMembers[i] == SteamUser.GetSteamID() ) {
					continue;
				}
				Renown.Entity player = PlayerScene.Instantiate<Renown.Entity>();
				player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( SteamLobby.Instance.LobbyMembers[i] ) );
				player.Set( "MultiplayerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
				player.Call( "SetOwnerId", (ulong)SteamLobby.Instance.LobbyMembers[i] );
				Players.Add( SteamLobby.Instance.LobbyMembers[i], player );
				PlayerList.AddChild( player );
			}
		}

		MercenaryLeaderboard.Init();

		GC.Collect( GC.MaxGeneration, GCCollectionMode.Aggressive );

		Console.PrintLine( "...Finished loading game" );
		GetNode<CanvasLayer>( "/root/LoadingScreen" ).Call( "FadeOut" );
	}

	private void SpawnPlayer( NetworkPlayer player ) {
		Godot.Collections.Array<Node> nodes = GetTree().GetNodesInGroup( "Checkpoints" );
		Checkpoint warpPoint = null;
		Godot.Vector2 position = ThisPlayer.GlobalPosition;
		float bestDistance = float.MaxValue;

		for ( int i = 0; i < nodes.Count; i++ ) {
			Checkpoint checkpoint = nodes[i] as Checkpoint;
			float dist = position.DistanceTo( checkpoint.GlobalPosition );
			
			if ( dist < bestDistance ) {
				bestDistance = dist;
				warpPoint = checkpoint;
			}
		}
		player.GlobalPosition = warpPoint.GlobalPosition;
	}
	private void OnPlayerJoined( ulong steamId ) {
		Console.PrintLine( string.Format( "Adding {0} to game...", steamId ) );

		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( Players.ContainsKey( userId ) ) {
			return;
		}
		
		Renown.Entity player = PlayerScene.Instantiate<Renown.Entity>();
		player.Set( "MultiplayerUsername", SteamFriends.GetFriendPersonaName( userId ) );
		player.Set( "MultiplayerId", (ulong)userId );
		player.Call( "SetOwnerId", (ulong)userId );
		player.GlobalPosition = new Godot.Vector2( -88720.0f, 53124.0f );
		SpawnPlayer( (NetworkPlayer)player );
		Players.Add( userId, player );
		PlayerList.AddChild( player );
	}
	private void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();

		CSteamID userId = (CSteamID)steamId;
		if ( userId == SteamUser.GetSteamID() ) {
			return;
		}
		
		Console.PrintLine(
			string.Format( "{0} has faded away...", ( Players[ userId ] as NetworkPlayer ).MultiplayerUsername )
		);
		PlayerList.CallDeferred( "remove_child", Players[ userId ] );
		Players[ userId ].QueueFree();
		Players.Remove( userId );
		SteamLobby.Instance.RemovePlayer( userId );
	}

	public override void _ExitTree() {
		Players.Clear();
		PlayerList.QueueFree();

		Faction.Cache.ClearCache();
		Settlement.Cache.ClearCache();
		WorldArea.Cache.ClearCache();

		Faction.Cache = null;
		Settlement.Cache = null;
		WorldArea.Cache = null;

		if ( Hellbreaker != null ) {
			Hellbreaker.QueueFree();
		}
	}
	public override void _Ready() {
		base._Ready();

		GetTree().CurrentScene = this;
		
		Players = new Dictionary<CSteamID, Renown.Entity>();

		ThisPlayer = GetNode<Player>( "Network/Players/Player0" );
		PauseMenu = GetNode<PauseMenu>( "CanvasLayer/PauseMenu" );
		PlayerList = GetNode<Node>( "Network/Players" );

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			ThisPlayer.SetupSplitScreen( 0 );
		}

		PauseMenu.Connect( "LeaveLobby", Callable.From( SteamLobby.Instance.LeaveLobby ) );
		SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );
		
		SceneLoadThread = new Thread( () => {
			PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" );
			Faction.Cache = new DataCache<Faction>( this, "Factions" );
			Settlement.Cache = new DataCache<Settlement>( this, "Settlements" );
			WorldArea.Cache = new DataCache<WorldArea>( this, "Locations" );

			if ( !ArchiveSystem.Instance.IsLoaded() ) {
				return;
			}

			Faction.Cache.Load();
			Settlement.Cache.Load();
			WorldArea.Cache.Load();
		} );

		ResourceLoadThread = new Thread( () => { ResourceCache.Cache( this, SceneLoadThread ); } );
		ResourceLoadThread.Start();

		ResourcesLoadingFinished += OnResourcesFinishedLoading;

		PhysicsServer2D.SetActive( true );

		SetProcess( false );
		SetProcessInternal( false );

		//
		// force the game to run at the highest priority possible
		//
		using ( Process process = Process.GetCurrentProcess() ) {
			process.PriorityBoostEnabled = true;

			switch ( OS.GetName() ) {
			case "Linux":
			case "Windows":
				process.ProcessorAffinity = System.Environment.ProcessorCount;
				break;
			};

			process.PriorityClass = ProcessPriorityClass.AboveNormal;
		}
	}
};
