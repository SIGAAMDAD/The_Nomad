using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Godot;
using Steamworks;

public partial class LevelData : Node2D {
	protected PauseMenu PauseMenu;

	protected Thread ResourceLoadThread;
	protected Thread SceneLoadThread;

	protected Dictionary<CSteamID, Renown.Entity> Players = null;	
	protected PackedScene PlayerScene = null;

	[Export]
	protected Player ThisPlayer;
	[Export]
	protected Node PlayerList = null;

	[Signal]
	public delegate void ResourcesLoadingFinishedEventHandler();

	protected virtual void OnResourcesFinishedLoading() {
	}
	protected virtual void OnPlayerJoined( ulong steamId ) {
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
		Players.Add( userId, player );
		PlayerList.AddChild( player );
	}
	protected virtual void OnPlayerLeft( ulong steamId ) {
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

	public override void _Ready() {
		base._Ready();

		GetTree().CurrentScene = this;

		if ( Input.GetConnectedJoypads().Count > 0 ) {
			ThisPlayer.SetupSplitScreen( 0 );
		}

		ResourcesLoadingFinished += OnResourcesFinishedLoading;

		PauseMenu = ResourceLoader.Load<PackedScene>( "res://scenes/menus/pause_menu.tscn" ).Instantiate<PauseMenu>();
		PauseMenu.Name = "PauseMenu";
		PauseMenu.Connect( "LeaveLobby", Callable.From( SteamLobby.Instance.LeaveLobby ) );
		AddChild( PauseMenu );

		if ( SettingsData.GetNetworkingEnabled() && GameConfiguration.GameMode != GameMode.ChallengeMode ) {
			SteamLobby.Instance.Connect( "ClientJoinedLobby", Callable.From<ulong>( OnPlayerJoined ) );
			SteamLobby.Instance.Connect( "ClientLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );
		}

		PhysicsServer2D.SetActive( true );

		SetProcess( false );
		SetProcessInternal( false );

		//
		// force the game to run at the highest priority possible
		//
		try {
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
		} catch ( Exception ) {
		}
	}
};