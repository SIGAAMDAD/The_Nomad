extends Node

var _steam_app_id:int = 480 # test appid "spacewar"
var _steam_id
var _steam_username:String = ""

func _on_user_stats_recieved( gameID: int, result: int, steamID: int ) -> void:
	print( "SteamAPI User Stats Recieved." )
	print( "GameID: %s" % gameID )
	print( "User SteamID: %s" % steamID )

func _on_user_stats_stored( app_id: ) -> void:
	pass

func _ready() -> void:
	if Engine.has_singleton( "Steam" ):
		OS.set_environment( "SteamAppID", str( _steam_app_id ) )
		OS.set_environment( "SteamGameID", str( _steam_app_id ) )
		Steam.steamInit( true, _steam_app_id )
		
		_steam_id = Steam.getSteamID()
		_steam_username = Steam.getFriendPersonaName( _steam_id )
		
		Steam.user_stats_received.connect( _on_user_stats_recieved )
		Steam.user_stats_stored.connect( _on_user_stats_stored )
		SteamAchievements.init()
		
		print( "SteamAPI initialized with username ", _steam_username )
	else:
		_steam_id = 0
		_steam_username = ""
