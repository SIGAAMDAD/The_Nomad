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

	public enum UpdateType : uint {
		WeaponSlots,
		WeaponsStack,
		AmmoLight,
		AmmoHeavy,
		AmmoPellets,
		Consumables,
		MiscItems,

		Count
	};

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
		ModeData.SetMode( (Mode.GameMode)(int)data[ "mode" ] );

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
		Player player = Players[ senderId ];

		player.SetHealth( (float)packet[ "health" ] );
		player.SetRage( (float)packet[ "rage" ] );
		player.SetFlags( (Player.PlayerFlags)packet[ "flags" ] );
		player.SetHandsUsed( (Player.Hands)packet[ "hands_used" ] );
		player.GlobalPosition = (Godot.Vector2)packet[ "position" ];
		player.SetArmAngle( (float)packet[ "rotation" ] );
	}
	public void ProcessPlayerUpdate( ulong senderId, Dictionary<string, object> packet ) {

	}

	public void SendPlayerUpdate( Player player, UpdateType type ) {
		switch ( type ) {
		case UpdateType.WeaponsStack:
			Update[ "data" ] = player.GetWeaponStack();
			break;
		case UpdateType.WeaponSlots:
			Update[ "slots" ] = player.GetWeaponSlots();
			break;
		};

	}

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
		Packets[ "rotation" ] = ThisPlayer.GetArmAngle();
		Packets[ "position" ] = ThisPlayer.GlobalPosition;

		SteamLobby.Instance.SendP2PPacket( 0, Packets );
	}

	public override void _Ready() {
		Network = GetNode<Node2D>( "Network" );
		SpawnTree = GetNode( "Network/Spawns" );
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		ClientHeartbeat = GetNode<Timer>( "ClientHeartbeatTimer" );
		ServerHeartbeat = GetNode<Timer>( "ServerHeartbeatTimer" );
		ServerHeartbeat.Connect( "timeout", Callable.From( OnServerHeartbeatTimeout ) );

		Players = new Dictionary<ulong, Player>();

		PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/Player.tscn" );
	}
};