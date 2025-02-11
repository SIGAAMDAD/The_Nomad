extends Control

@onready var _player_list:VBoxContainer = $MarginContainer/PlayerList
@onready var _start_game_button:Button = $StartGameButton

@onready var _loading_screen:Resource = preload( "res://scenes/menus/loading_screen.tscn" )
@onready var _new_loading_screen:Node = null

func _on_start_game() -> void:
	pass

func init() -> void:
#	Steam.lobby_chat_update.connect( _on_lobby_chat_update )
	SteamLobby.start_game.connect( _on_start_game )
	
	if SteamLobby._is_host:
		_start_game_button.show()
	else:
		_start_game_button.hide()

func clear_player_list() -> void:
	for player in _player_list.get_children():
		_player_list.remove_child( player )
		player.queue_free()

func check_player_list() -> void:
	if Steam.getNumLobbyMembers( SteamLobby._lobby_id ) == _player_list.get_child_count():
		return
	
	print( "updating lobby player list..." )
	SteamLobby.get_lobby_members()
	clear_player_list()
	
	for member in SteamLobby._lobby_members:
		var player:Label = Label.new()
		player.set_size( Vector2( 240, 20 ) )
		player.text = Steam.getFriendPersonaName( member[ "steam_id" ] )
		
		_player_list.add_child( player )

func _process( _delta: float ) -> void:
	check_player_list()

func on_loaded_map() -> void:
	GameConfiguration.LoadedLevel.ChangeScene()
	_new_loading_screen.hide()
	SoundManager.stop_music( 0.5 )
	self.hide()

func _on_start_game_button_pressed() -> void:
	return
	if !SteamLobby._is_host:
		print( "Not the host of the lobby, cannot start the game." )
		return
	# TODO: maybe make a vote to start feature?
	SteamLobby.send_p2p_packet( 0, { "message": "start_game" } )

func _on_exit_lobby_button_pressed() -> void:
	print( "Leaving the lobby..." )
	emit_signal( "leave_lobby" )
