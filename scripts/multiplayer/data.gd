class_name MultiplayerData extends Node2D

@onready var _network = $Network

class Team:
	var _score:int = 0
	var _players:Array[ Player ] = []
	var _index:int = 0

var _mode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Bloodbath
var _players:Dictionary = {}

func init( mode: MultiplayerMode.GameMode ) -> void:
	_mode = mode
	
	for member in SteamLobby._lobby_members:
		var steamId:int = member[ "steam_id" ]
		
		print( "Adding player to map with steam_id %s..." % steamId )
		
		_players[ steamId ] = Player.new()
		_network.add_child( _players[ steamId ] )

func _process( _delta: float ) -> void:
	# TODO: optimize?
	var packet:Dictionary = { "message": "data", "steam_id": SteamManager._steam_id }
	for player in _players.values():
		packet[ "data" ] = {
			"position": player.global_position,
			"arotation": player._arm_rotation,
			"drotation": player._draw_rotation,
		};
		SteamLobby.send_p2p_packet( 0, packet )
