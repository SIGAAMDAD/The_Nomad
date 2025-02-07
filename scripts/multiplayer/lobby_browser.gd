extends Control

@onready var _host_game:Button = $ControlBar/HostButton
@onready var _refresh_lobbies:Button = $ControlBar/RefreshButton
@onready var _matchmake:Button = $ControlBar/MatchmakeButton

@onready var _matchmaking_spinner:Spinner = $LobbyMetadataContainer/MatchMakingSpinner
@onready var _matchmaking_label:Label = $LobbyMetadataContainer/MatchMakingLabel

@onready var _lobby_table:VBoxContainer = $LobbyList/Lobbies

var _matchmaking_phase:int = 0

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

func matchmaking_loop() -> void:
	if _matchmaking_phase < 4:
		Steam.addRequestLobbyListDistanceFilter( _matchmaking_phase )
		
		# get the list
		Steam.requestLobbyList()
	else:
		push_error( "[STEAM] failed to automatically match player with a lobby, please try again." )

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

func auto_matchmake() -> void:
	var attemptingJoin := false
	
	for lobby in SteamLobby._lobby_list:
		var lobbyName := Steam.getLobbyData( lobby, "name" )
		var lobbyMaxPlayers := Steam.getLobbyMemberLimit( lobby )
		var lobbyMemberCount := Steam.getNumLobbyMembers( lobby )
		
		###
		# Add other filters for things like game modes, etc.
		# Since this is an example, we cannot set game mode or text match features.
		# However, much like lobby_name, you can use Steam.getLobbyData to get other
		# preset lobby defining data to append to the next if statement.
		###
		
		if lobbyMemberCount < lobbyMaxPlayers && !attemptingJoin:
			attemptingJoin = true
			_matchmaking_label.text = "FOUND MATCH..."
			print( "Attempting to join lobby %s..." % lobby )
			Steam.joinLobby( lobby )

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
	_matchmaking_spinner.show()
	_matchmaking_label.show()
	
	_matchmaking_label.text = "SORTING CONTRACTS..."
	
	_matchmaking_phase = 0

func _on_join_button_pressed() -> void:
	pass # Replace with function body.
