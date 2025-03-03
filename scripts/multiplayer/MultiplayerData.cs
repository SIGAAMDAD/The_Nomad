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
		public System.Collections.Generic.List<Player> Players = new System.Collections.Generic.List<Player>();
		public int Score = 0;
		public int Index = 0;

		public Team() {
		}
	};

	public Team BlueTeam = null;
	public Team RedTeam = null;

	private Mode ModeData = null;
	private System.Collections.Generic.Dictionary<ulong, Player> Players = null;

	// prebuild packets
	private System.Collections.Generic.Dictionary<string, object> GameData = new System.Collections.Generic.Dictionary<string, object>();
	private System.Collections.Generic.Dictionary<string, object> Packets = new System.Collections.Generic.Dictionary<string, object>();

	public void ProcessHeartbeat( System.Collections.Generic.Dictionary<string, object>  data ) {
		ModeData.SetMode( (Mode.GameMode)(int)data[ "mode" ] );

		foreach ( var player in Players.Values ) {
			System.Collections.Generic.Dictionary<string, object> values = (System.Collections.Generic.Dictionary<string, object>)data[ player.MultiplayerId.ToString() ];

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
	public void ProcessClientData( ulong senderId, System.Collections.Generic.Dictionary<string, object> packet ) {

	}
	public void ProcessPlayerUpdate( ulong senderId, System.Collections.Generic.Dictionary<string, object> packet ) {

	}

	public void SendPlayerUpdate( Player player ) {
	}

	private void OnServerHeartbeatTimeout() {
		if ( !SteamLobby.Instance.IsOwner() ) {
			return; // we're not the host
		}

		GameData[ "mode" ] = ModeData.GetMode();

		foreach ( var player in Players.Values ) {
			System.Collections.Generic.Dictionary<string, object> playerData = (System.Collections.Generic.Dictionary<string, object>)GameData[ player.MultiplayerId.ToString() ];

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
	}

	public override void _Ready() {
		Network = GetNode<Node2D>( "Network" );
		SpawnTree = GetNode( "Network/Spawns" );
		PauseMenu = GetNode<Control>( "CanvasLayer/PauseMenu" );
		ClientHeartbeat = GetNode<Timer>( "ClientHeartbeatTimer" );
		ServerHeartbeat = GetNode<Timer>( "ServerHeartbeatTimer" );
		ServerHeartbeat.Connect( "timeout", Callable.From( OnServerHeartbeatTimeout ) );

		Players = new System.Collections.Generic.Dictionary<ulong, Player>();

		PlayerScene = ResourceLoader.Load<PackedScene>( "res://scenes/Player.tscn" );
	}
};