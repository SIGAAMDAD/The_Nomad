extends Control

@onready var _host_game:Button = $ControlBar/HostButton
@onready var _refresh_lobbies:Button = $ControlBar/RefreshButton
@onready var _matchmake:Button = $ControlBar/MatchmakeButton

@onready var _matchmaking_spinner:Spinner = $MatchMakingSpinner
@onready var _matchmaking_label:Label = $MatchMakingLabel

@onready var _lobby_table:VBoxContainer = $LobbyList/Lobbies

@onready var _loading_screen:Resource = preload( "res://scenes/menus/loading_screen.tscn" )
@onready var _new_loading_screen:Node = null

var _matchmaking_phase:int = 0

signal on_host_game()

class LobbyData:
	var _map:String = ""
	var _player_count:int = 0
	var _max_players:int = 0
	var _game_mode:MultiplayerMode.GameMode = MultiplayerMode.GameMode.Bloodbath
	
	func _init( map: String, playerCount: int, maxPlayers: int, gameMode: String ) -> void:
		_map = map
		_player_count = playerCount
#		_game_mode = MultiplayerMode.GameMode.get( gameMode )

var _lobby_list:Dictionary = {}

func _ready() -> void:
	SteamLobby.lobby_joined.connect( _on_lobby_joined )
	Steam.lobby_joined.connect( _on_lobby_joined )

func on_loaded_map() -> void:
	GameConfiguration.LoadedLevel.ChangeScene()
	_new_loading_screen.hide()
	_new_loading_screen.queue_free()
	SoundManager.stop_music( 0.5 )
	self.hide()

func _on_lobby_joined( lobbyId: int ) -> void:
	print( "...joined" )
	SteamLobby._lobby_id = lobbyId
	
	_new_loading_screen = _loading_screen.instantiate()
	get_tree().get_root().add_child( _new_loading_screen )
	
	var modeName:String = ""
	match SteamLobby._lobby_gamemode:
		MultiplayerMode.GameMode.Bloodbath:
			modeName = "bloodbath"
		MultiplayerMode.GameMode.TeamBrawl:
			modeName = "teambrawl"
		MultiplayerMode.GameMode.CaptureTheFlag:
			modeName = "ctf"
	
	print( "Loading map..." )
	GameConfiguration.LoadedLevel = AsyncScene.new( "res://levels/" + MultiplayerMapManager._map_cache[ SteamLobby._lobby_map ]._filename + "_mp_" + modeName + ".tscn", AsyncScene.LoadingSceneOperation.Replace )
	GameConfiguration.LoadedLevel.OnComplete.connect( on_loaded_map )

func matchmaking_loop() -> void:
	if _matchmaking_phase < 4:
		Steam.addRequestLobbyListDistanceFilter( _matchmaking_phase )
		
		# get the list
		Steam.requestLobbyList()
	else:
		push_error( "[STEAM] failed to automatically match player with a lobby, please try again." )

func _on_join_game( lobbyId: int ) -> void:
	print( "Joining lobby %s..." % lobbyId )
	emit_signal( "set_lobby_room_menu" )
	Steam.joinLobby( lobbyId )

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
		
		button.connect( "pressed", Callable( self, "_on_join_game" ).bind( lobby ) )
		
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

func _process(  delta: float ) -> void:
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
