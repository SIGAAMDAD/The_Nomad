class_name MultiplayerData extends Node2D

@onready var _network = $Network
@onready var _spawn_tree:Node = $Network/Spawns
@onready var _pause_menu = $CanvasLayer/PauseMenu

var _player_scene := preload( "res://scenes/Player.tscn" )

class Team:
	var _score:int = 0
	var _players:Array[ Player ] = []
	var _index:int = 0

var _mode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Bloodbath
var _players:Dictionary = {}

func process_packet( data: Dictionary, sender: int ) -> void:
	var player:Player = _players[ sender ]
	
	player.global_position = data[ "position" ]
	player._arm_rotation = data[ "rotation" ]

func spawn_player( playerId: int ) -> void:
	var player:Player = _players[ playerId ]
	var spawn := _spawn_tree.get_child( randi_range( 0, _spawn_tree.get_child_count() - 1 ) )
	
	player._health = 100.0
	player.global_position = spawn.global_position
	
	print_debug( "spawning player %s..." % playerId )

func on_player_joined( steamId: int ) -> void:
	print( "Adding %s to game..." % steamId )
	
	_players[ steamId ] = _player_scene.instantiate()
	_players[ steamId ]._multiplayer_username = Steam.getFriendPersonaName( steamId )
	_players[ steamId ]._multiplayer_id = steamId
	$Network.add_child( _players[ steamId ] )
	
	if steamId == SteamManager._steam_id:
		spawn_player( SteamManager._steam_id )
	
	SteamLobby.get_lobby_members()
	for member in SteamLobby._lobby_members:
		if !_players.has( member[ "steam_id" ] ) && member[ "steam_id" ] != steamId:
			on_player_joined( member[ "steam_id" ] )

func on_player_left( steamId: int ) -> void:
	SteamLobby.get_lobby_members()
	Console.print_line( "Player " + var_to_str( SteamManager._steam_id ) + " has fled the scene...", true )
	$Network.remove_child( _players[ steamId ] )
	_players[ steamId ].queue_free()
	_players.erase( steamId )

func _on_member_list_updated() -> void:
	print( "updating member list..." )
	return
	
	for member in SteamLobby._lobby_members:
		if !_players.has( member[ "steam_id" ] ):
			on_player_joined( member[ "steam_id" ] )
	
	for player in _players.values():
		# left the lobby
		if !SteamLobby._lobby_members.has( player._multiplayer_id ):
			$Network.remove_child( _players[ player._multiplayer_id ] )
			_players[ player._multiplayer_id ].queue_free()
			_players.erase( player._multiplayer_id )

func on_chat_message_received( senderSteamId: int, message: String ) -> void:
	Console.print_line( "[" + _players[ senderSteamId ]._multiplayer_username + "] " + message )

func _cmd_chat_message_send( message: String ) -> void:
	Steam.sendLobbyChatMsg( SteamLobby._lobby_id, message )

func _ready() -> void:
	_pause_menu.leave_lobby.connect( SteamLobby.leave_lobby )
	SteamLobby.client_left_lobby.connect( on_player_left )
	SteamLobby.client_joined_lobby.connect( on_player_joined )
	SteamLobby.lobby_members_updated.connect( _on_member_list_updated )
	SteamLobby.chat_message_received.connect( on_chat_message_received )
	
	var message:String
	Console.add_command( "send_message", _cmd_chat_message_send, [ message ], 1 )
	
	on_player_joined( SteamManager._steam_id )

func _process( _delta: float ) -> void:
	# TODO: optimize?
	var packet:Dictionary = { "message": "packet", "remote_steam_id": SteamManager._steam_id }
	var player:Player = _players[ SteamManager._steam_id ]
	
	packet[ "packet" ] = {
		"position": player.global_position,
		"rotation": player._arm_rotation,
	};
	SteamLobby.send_p2p_packet( 0, packet )
