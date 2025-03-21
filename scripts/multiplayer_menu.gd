extends Control

@onready var _host_game:Button = $ControlBar/HostButton
@onready var _refresh_lobbies:Button = $ControlBar/RefreshButton
@onready var _lobby_control:ScrollContainer = $LobbyList
@onready var _lobby_table:VBoxContainer = $LobbyList/Lobbies

class LobbyData:
	var _map:String = ""
	var _player_count:int = 0
	var _game_mode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Massacre
	
	func _init( map: String, playerCount: int, gameMode: String ) -> void:
		_map = map
		_player_count = playerCount
		_game_mode = MultiplayerMode.GameMode[ gameMode ]

var _lobby_list:Dictionary = {}

func _on_refresh_pressed() -> void:
	if _lobby_control.get_child_count() > 0:
		for lobby in _lobby_control.get_children():
			lobby.queue_free()

func _on_host_pressed() -> void:
	SteamLobby.create_lobby()
	_host_game.hide()
	_lobby_table.hide()

func join_lobby( lobbyId: int ) -> void:
	Steam.joinLobby( lobbyId )
	_host_game.hide()
	_lobby_table.hide()

func _ready() -> void:
	_host_game.pressed.connect( _on_host_pressed )

func get_lobby_list() -> void:
	for lobby in SteamLobby._lobby_list:
		var lobbyName := Steam.getLobbyData( lobby, "name" )
		
		var button = Button.new()
		button.set_text( lobbyName )
		button.set_size( Vector2( 10, 2 ) )
		button.pressed.connect( Callable( self, "join_lobby" ).bind( lobby ) )
		
		var lobbyMemberCount := Steam.getNumLobbyMembers( lobby )
		var lobbyMap := Steam.getLobbyData( lobby, "map" )
		var lobbyGameMode := Steam.getLobbyData( lobby, "gamemode" )
		
		_lobby_table.add_child( button )
#		_lobby_list[ lobbyName ] = LobbyData.new( lobbyMap, lobbyMemberCount, lobbyGameMode )

func _process( delta: float ) -> void:
	if !_lobby_table.get_child_count():
		get_lobby_list()
