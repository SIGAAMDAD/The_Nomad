extends Node2D

@onready var _network = $Network

var _mode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Massacre
var _players:Dictionary = {}

func _ready() -> void:
	for member in SteamLobby._lobby_members:
		var steamId:int = member[ "steam_id" ]
		
		print( "Adding player to map with steam_id %s..." % steamId )
		
		_players[ steamId ] = Player.new()
		_network.add_child( _players[ steamId ] )

func _process( delta: float ) -> void:
	# TODO: optimize?
	var packet:Dictionary = { "message": "data" }
	for member in SteamLobby._lobby_members:
		var steamId:int = member[ "steam_id" ]
		
		packet[ "steam_id" ] = steamId;
		for player in _players.values():
			packet[ "data" ] = {
				"position": player.global_position,
				"angle": player._arm_rotation,
			};
			SteamLobby.send_p2p_packet( steamId, packet )
