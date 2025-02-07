extends Control

@onready var _host_game:Button = $ControlBar/HostButton
@onready var _refresh_lobbies:Button = $ControlBar/RefreshButton
@onready var _matchmake:Button = $ControlBar/MatchmakeButton

@onready var _lobby_table:VBoxContainer = $LobbyList/Lobbies

signal on_host_game()

class LobbyData:
	var _map:String = ""
	var _player_count:int = 0
	var _max_players:int = 0
	var _game_mode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Massacre
	
	func _init( map: String, playerCount: int, maxPlayers: int, gameMode: String ) -> void:
		_map = map
		_player_count = playerCount
#		_game_mode = MultiplayerMode.GameMode.get( gameMode )

var _lobby_list:Dictionary = {}

func join_lobby( lobbyId: int ) -> void:
	Steam.joinLobby( lobbyId )
	_host_game.hide()
	_lobby_table.hide()

func get_lobby_list() -> void:
	for lobby in SteamLobby._lobby_list:
		var lobbyName := Steam.getLobbyData( lobby, "name" )
		
		var lobbyMemberCount := Steam.getNumLobbyMembers( lobby )
		var lobbyMaxMemberCount := Steam.getLobbyMemberLimit( lobby )
		var lobbyMap := Steam.getLobbyData( lobby, "map" )
		var lobbyGameMode := Steam.getLobbyData( lobby, "gamemode" )
		
		var button = Button.new()
		button.set_text( lobbyName )
		button.set_size( Vector2( 240, 20 ) )
		
		_lobby_table.add_child( button )
		_lobby_list[ lobbyName ] = LobbyData.new( lobbyMap, lobbyMemberCount, lobbyMaxMemberCount, lobbyGameMode )

func _process( delta: float ) -> void:
	if !_lobby_table.get_child_count():
		get_lobby_list()

func _on_refresh_button_pressed() -> void:
	if _lobby_table.get_child_count() > 0:
		for lobby in _lobby_table.get_children():
			lobby.queue_free()
	
	Steam.requestLobbyList()

func _on_host_button_pressed() -> void:
	emit_signal( "on_host_game" )

func _on_matchmake_button_pressed() -> void:
	pass # Replace with function body.

func _on_join_button_pressed() -> void:
	pass # Replace with function body.
