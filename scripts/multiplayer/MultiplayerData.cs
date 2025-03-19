using System.Collections.Generic;
using Godot;
using Multiplayer;

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
	private Dictionary<ulong, Player> Players = null;
	private Player ThisPlayer;
	
	private Node PlayerList = null;
	
	private Control PauseMenu = null;

	// prebuild packets
	private Dictionary<string, object> GameData = new Dictionary<string, object>();
	private Dictionary<string, object> Packets = new Dictionary<string, object>();
	private Dictionary<string, object> Update = new Dictionary<string, object>();
	
	public Mode.GameMode GetMode() {
		return ModeData.GetMode();
	}
	public Dictionary<ulong, Player> GetPlayers() {
		return Players;
	}

	public void ProcessHeartbeat( Dictionary<string, object>  data ) {
		foreach ( var player in Players.Values ) {
			Dictionary<string, object> values = (Dictionary<string, object>)data[ player.MultiplayerId.ToString() ];

			player.MultiplayerKills = (uint)values[ "kills" ];
			player.MultiplayerDeaths = (uint)values[ "deaths" ];

			switch ( ModeData.GetMode() ) {
			case Mode.GameMode.CaptureTheFlag:
				player.MultiplayerFlagCaptures = (uint)values[ "flag_captures" ];
				player.MultiplayerFlagReturns = (uint)values[ "flag_returns" ];
				break;
			case Mode.GameMode.KingOfTheHill:
				player.MultiplayerHilltime = (float)values[ "hilltime" ];
				break;
			};
		}
	}
	public void ProcessClientData( ulong senderId, Dictionary<string, object> packet ) {
		NetworkPlayer player = Players[ senderId ] as NetworkPlayer;
		
		player.SetHealth( (float)packet[ "health" ] );
		player.SetRage( (float)packet[ "rage" ] );
		player.SetFlags( (Player.PlayerFlags)packet[ "flags" ] );
		player.SetHandsUsed( (Player.Hands)packet[ "hands_used" ] );
		player.GlobalPosition = (Godot.Vector2)packet[ "position" ];
		
		PlayerSystem.Arm leftArm = player.GetArmLeft();
		PlayerSystem.Arm rightArm = player.GetArmRight();
		
		leftArm.SetMode( (WeaponEntity.Property)packet[ "arm_left_mode" ] );
		leftArm.SetWeapon( (int)packet[ "arm_left_slot" ] );
		leftArm.GlobalRotation = (float)packet[ "arm_left_rotation" ];
		
		rightArm.SetMode( (WeaponEntity.Property)packet[ "arm_right_mode" ] );
		rightArm.SetWeapon( (int)packet[ "arm_right_slot" ] );
		rightArm.GlobalRotation = (float)packet[ "arm_right_rotation" ];
	}
	public void ProcessStatusUpdate( Dictionary<string, object> packet ) {
		Player player = Players[ (ulong)packet[ "id" ] ];
		player.SetHealth( (float)packet[ "health" ] );
		player.SetFlags( (PlayerFlags)packet[ "flags" ] );
	}
	
	/// <summary>
	///	sends gamestate data from the host to all the clients
	/// </summary>
	private void OnServerHeartbeatTimeout() {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return; // we're not the host
		}

		GameData[ "mode" ] = ModeData.GetMode();

		foreach ( var player in Players.Values ) {
			Dictionary<string, object> playerData = (Dictionary<string, object>)GameData[ player.MultiplayerId.ToString() ];

			playerData[ "kills" ] = player.MultiplayerKills;
			playerData[ "deaths" ] = player.MultiplayerDeaths;
			switch ( ModeData.GetMode() ) {
			case Mode.GameMode.Bloodbath:
			case Mode.GameMode.Duel:
			case Mode.GameMode.TeamBrawl:
				break;
			case Mode.GameMode.KingOfTheHill:
				playerData[ "flag_captures" ] = player.MultiplayerFlagCaptures;
				playerData[ "flag_returns" ] = player.MultiplayerFlagReturns;
				break;
			case Mode.GameMode.CaptureTheFlag:
				playerData[ "hilltime" ] = player.MultiplayerHilltime;
				break;
			};
		}
		SteamLobby.Instance.SendP2PPacket( 0, GameData );
	}
	private void OnClientHeartbeatTimeout() {
		Packets[ "health" ] = ThisPlayer.GetHealth();
		Packets[ "rage" ] = ThisPlayer.GetRage();
		Packets[ "flags" ] = ThisPlayer.GetFlags();
		Packets[ "hands_used" ] = ThisPlayer.GetHandsUsed();
		Packets[ "position" ] = ThisPlayer.GlobalPosition;
		Packets[ "weapon" ] = (string)ThisPlayer.GetWeaponSlots()[ ThisPlayer.GetCurrentWeapon() ].GetWeapon().Data.Get( "id" );
		
		PlayerSystem.Arm leftArm = ThisPlayer.GetArmLeft();
		Packets[ "arm_left_rotation" ] = leftArm.GlobalRotation;
		Packets[ "arm_left_mode" ] = leftArm.GetMode();
		
		PlayerSystem.Arm rightArm = ThisPlayer.GetArmRight();
		Packets[ "arm_right_rotation" ] = rightArm.GlobalRotation;
		Packets[ "arm_right_mode" ] = rightArm.GetMode();
		
		SteamLobby.Instance.SendP2PPacket( 0, Packets );
	}

	private void OnPlayerJoined( ulong steamId ) {
		GetNode( "/root/Console" ).Call( "print_line", "Adding " + steamId + " to game..." );
		if ( Players.ContainsKey( steamId ) ) {
			return;
		}
		
		Player player = PlayerScene.Instantiate<Player>();
		player.MutliplayerUserName = Steam.GetFriendPersonaName( steamId );
		player.MutliplayerId = steamId;
		SpawnPlayer( player );
		Players.Add( steamId, player );
		PlayerList.AddChild( player );
	}
	private void OnPlayerLeft( ulong steamId ) {
		SteamLobby.Instance.GetLobbyMembers();
		
		GetNode( "/root/Console" ).Call( "print_line", Players[ steamId ].MultiplayerUserName + " has faded away..." );
		PlayerList.RemoveChild();
		Players[ steamId ].QueueFree();
		Players.Remove( steamId );
	}
	
	public override void _Ready() {
		Network = GetNode<Node2D>( "Network" );
		SpawnTree = GetNode( "Network/Spawns" );
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		
		ServerHeartbeat = GetNode<Timer>( "ServerHeartbeatTimer" );
		ServerHeartbeat.Connect( "timeout", Callable.From( OnServerHeartbeatTimeout ) );

		Players = new Dictionary<ulong, Player>();

		PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/Player.tscn" );
		ThisPlayer = GetNode<Player>( "Network/Players/Player0" );
		
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		PlayerList = GetNode<Node>( "Network/Players" );
		
		PauseMenu.Connect( "leave_lobby", Callable.From( SteamLobby.LeaveLobby ) );
		SteamLobby.Instance.Connect( "PlayerJoined", Callable.From( OnPlayerJoined ) );
		SteamLobby.Instance.Connect( "PlayerLeftLobby", Callable.From( OnPlayerLeft ) );
	}
};
