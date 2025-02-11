class_name MultiplayerData extends Node2D

@onready var _network = $Network
@onready var _pause_menu = $CanvasLayer/PauseMenu

var _player_scene := preload( "res://scenes/Player.tscn" )

class Team:
	var _score:int = 0
	var _players:Array[ Player ] = []
	var _index:int = 0

var _mode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Bloodbath
var _players:Dictionary = {}
var _player_data:Array[ Player ] = []

func on_player_joined( steamId: int ) -> void:
	print( "Adding %s to game..." % steamId )
	if _players.has( steamId ):
		return
	
	SteamLobby.get_lobby_members()
	
	_players[ steamId ] = _player_scene.instantiate()
	_players[ steamId ].global_position.x = 635
	_players[ steamId ].global_position.y = 574
	_players[ steamId ]._multiplayer_username = Steam.getFriendPersonaName( steamId )
	_players[ steamId ]._multiplayer_id = steamId
	$Network/Players.add_child( _players[ steamId ] )

func on_player_left( steamId: int ) -> void:
	SteamLobby.get_lobby_members()
	Console.print_line( "Player " + var_to_str( SteamManager._steam_id ) + " has fled the scene...", true )
	$Network/Players.remove_child( _players[ steamId ] )
	_players[ steamId ].queue_free()

func on_chat_message_received( senderSteamId: int, message: String ) -> void:
	Console.print_line( "[" + _players[ senderSteamId ]._multiplayer_username + "] " + message )
	SteamNetwork.rpc_on_server( self, "set_player_position", [ _players[ senderSteamId ].global_position ] )

func _cmd_chat_message_send( message: String ) -> void:
	Steam.sendLobbyChatMsg( SteamLobby._lobby_id, message )

func _ready() -> void:
	_pause_menu.leave_lobby.connect( SteamLobby.leave_lobby )
	SteamNetwork.player_left_lobby.connect( on_player_left )
	SteamNetwork.player_joined_lobby.connect( on_player_joined )
	SteamLobby.chat_message_received.connect( on_chat_message_received )
	
	var message:String
	Console.add_command( "send_message", _cmd_chat_message_send, [ message ], 1 )

func init() -> void:
	if _players.has( SteamLobby._lobby_owner_id ):
		return
	
	on_player_joined( SteamLobby._lobby_owner_id )

func _process( _delta: float ) -> void:
	init()
	
	return
	
	# TODO: optimize?
	var packet:Dictionary = { "message": "data", "steam_id": SteamManager._steam_id }
	for player in _players.values():
		packet[ "data" ] = {
			"position": player.global_position,
			"arotation": player._arm_rotation,
			"drotation": player._draw_rotation,
		};
		SteamLobby.send_p2p_packet( 0, packet )
