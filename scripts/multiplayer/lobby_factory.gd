extends Control

@onready var _name:LineEdit = $MarginContainer/VBoxContainer/NameContainer/NameLineEdit
@onready var _max_players:HSlider = $MarginContainer/VBoxContainer/MaxPlayersContainer/MaxPlayersHSlider
@onready var _map_list:OptionButton = $MarginContainer/VBoxContainer/MapContainer/MapOptionButton
@onready var _game_mode_list:OptionButton = $MarginContainer/VBoxContainer/GameModeContainer/GameModeOptionButton

@onready var _loading_screen:Resource = preload( "res://scenes/menus/loading_screen.tscn" )
@onready var _new_loading_screen:Node = null

func _ready() -> void:
	MultiplayerMapManager.init()
	
	for map in MultiplayerMapManager._map_cache:
		_map_list.add_item( map._name )
	
	for mode in MultiplayerMode.GameMode.values():
		_game_mode_list.add_item( MultiplayerMode.MODE_NAMES[ mode ] )

func on_loaded_map() -> void:
	GameConfiguration.LoadedLevel.ChangeScene()
	_new_loading_screen.hide()
	_new_loading_screen.queue_free()
	
	SoundManager.stop_music( 0.5 )
	self.hide()

func _on_create_button_pressed() -> void:
	print( "Creating lobby..." )
	
	SteamLobby.create_lobby()
	self.hide()
	
	SteamLobby._lobby_name = _name.text
	SteamLobby._lobby_max_members = _max_players.value
	SteamLobby._lobby_map = _map_list.selected
	SteamLobby._lobby_mapname = MultiplayerMapManager._map_cache[ _map_list.selected ]._name
	SteamLobby._is_host = true
	GameConfiguration._game_mode = GameConfiguration.GameMode.Multiplayer
	
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
	
	print( "Starting game..." )
	GameConfiguration.LoadedLevel = AsyncScene.new( "res://levels/" + MultiplayerMapManager._map_cache[ SteamLobby._lobby_map ]._filename + "_mp_" + modeName + ".tscn", AsyncScene.LoadingSceneOperation.Replace )
	GameConfiguration.LoadedLevel.OnComplete.connect( on_loaded_map )
