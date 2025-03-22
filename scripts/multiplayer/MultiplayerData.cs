using System.Collections.Generic;
using Godot;
using Multiplayer;
using Steamworks;

public enum PacketType : uint {
	UpdatePlayer,
	DamagePlayer,

	UpdateMob,
	DamageMob,

	UpdateBot,
	DamageBot,
	
	Count
};

public partial class MultiplayerData : Node2D {
	private Node2D Network = null;
	private Node SpawnTree = null;
	private Control PauseMenu = null;
	private Timer ClientHeartbeat = null;
	private Timer ServerHeartbeat = null;

	private PackedScene PlayerScene = null;

	public class Team {
		public List<Player> Players = new List<Player>();
		public int Score = 0;
		public int Index = 0;

		public Team() {
		}
	};

	public Team BlueTeam = null;
	public Team RedTeam = null;

	private Mode ModeData = null;
	private Player ThisPlayer;
	private Dictionary<CSteamID, NetworkPlayer> Players = null;	
	
	private Node PlayerList = null;

	// prebuild packets
	private Dictionary<string, object> GameData = new Dictionary<string, object>();
	private Dictionary<string, object> Packets = new Dictionary<string, object>();
	private Dictionary<string, object> Update = new Dictionary<string, object>();
	
	public Mode.GameMode GetMode() {
		return ModeData.GetMode();
	}
	public Dictionary<CSteamID, NetworkPlayer> GetPlayers() {
		return Players;
	}

	public void ProcessHeartbeat( Dictionary<string, object> data ) {
	}
	public void ProcessClientData( ulong senderId, Dictionary<string, object> packet ) {
		switch ( (PacketType)packet[ "type" ] ) {
		case PacketType.UpdatePlayer:
			Players[ (CSteamID)senderId ].Update( packet );
			break;
		case PacketType.DamagePlayer:
			ThisPlayer.Damage( (CharacterBody2D)packet[ "attacker" ], (float)packet[ "amount" ] );
			break;
		};
	}
	
	/// <summary>
	///	sends gamestate data from the host to all the clients
	/// </summary>
	private void OnServerHeartbeatTimeout() {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return; // we're not the host
		}

		GameData[ "mode" ] = ModeData.GetMode();
		SteamLobby.Instance.SendP2PPacket( CSteamID.Nil, GameData, SteamLobby.MessageType.ServerData );
	}
	private void OnClientHeartbeatTimeout() {
		ThisPlayer.SendPacket();
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
	
	public override void _Ready() {
		Network = GetNode<Node2D>( "Network" );
		SpawnTree = GetNode( "Network/Spawns" );
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		
		ServerHeartbeat = GetNode<Timer>( "ServerHeartbeatTimer" );
		if ( (uint)GetNode( "/root/GameConfiguration" ).Get( "_game_mode" ) == (uint)Player.GameMode.Multiplayer ) {
			ServerHeartbeat.Connect( "timeout", Callable.From( OnServerHeartbeatTimeout ) );
		}
		
		Players = new Dictionary<CSteamID, NetworkPlayer>();

		PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/network_player.tscn" );
		ThisPlayer = GetNode<Player>( "Network/Players/Player0" );
		
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		PlayerList = GetNode<Node>( "Network/Players" );
		
		PauseMenu.Connect( "leave_lobby", Callable.From( SteamLobby.Instance.LeaveLobby ) );
		SteamLobby.Instance.Connect( "PlayerJoined", Callable.From<ulong>( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "PlayerLeftLobby", Callable.From<ulong>( OnPlayerLeft ) );
	}
};