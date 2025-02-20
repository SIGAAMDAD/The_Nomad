extends Node

var _lifetime_kills:int = 0
var _lifetime_deaths:int = 0
var _lifetime_wins:int = 0
var _lifetime_losses:int = 0
var _lifetime_games:int = 0
var _lifetime_kdr:float = 0.0
var _userstats_recieved:bool = false

func save_stats() -> void:
	var file := FileAccess.open( "user://stats.dat", FileAccess.WRITE )
	file.store_int( _lifetime_kills )
	file.store_int( _lifetime_deaths )
	file.store_int( _lifetime_wins )
	file.store_int( _lifetime_losses )
	file.store_int( _lifetime_games )
	file.store_float( _lifetime_kdr )
	file.close()
	
	if _userstats_recieved:
		Steam.setStatInt( "LifeTime Kills", _lifetime_kills )
		Steam.setStatInt( "LifeTime Deaths", _lifetime_deaths )
		Steam.setStatInt( "LifeTime Wins", _lifetime_wins )
		Steam.setStatInt( "LifeTime Losses", _lifetime_losses )
		Steam.setStatInt( "LifeTime Games", _lifetime_games )
		Steam.setStatFloat( "LifeTime KDR", _lifetime_kdr )
		
		if !Steam.storeStats():
			push_error( "Steam.storeStats failed!" )

func load_stats() -> void:
	Steam.requestUserStats( SteamManager._steam_id )

func _on_request_user_stats( game: int, result: Steam.Result, user: int ) -> void:
	if game != SteamManager._steam_app_id:
		return
	elif result != Steam.Result.RESULT_OK:
		push_error( "_on_request_user_stats: couldn't fetch user stats!" )
		return
	
	_userstats_recieved = true
	
	_lifetime_kills = Steam.getStatInt( "LifeTime Kills" )
	_lifetime_deaths = Steam.getStatInt( "LifeTime Deaths" )
	_lifetime_wins = Steam.getStatInt( "LifeTime Wins" )
	_lifetime_losses = Steam.getStatInt( "LifeTime Losses" )
	_lifetime_games = Steam.getStatInt( "LifeTime Games" )
	_lifetime_kdr = Steam.getStatFloat( "LifeTime KDR" )

func _on_user_stats_stored( game: int, result: Steam.Result ) -> void:
	if game != SteamManager._steam_app_id:
		return
	elif result != Steam.Result.RESULT_OK:
		push_error( "_on_user_stats_stored: couldn't store user stats!" )
		return

func _ready() -> void:
	print( "Getting multiplayer statistics for \"%s\"..." % SteamManager._steam_username )
	
	Steam.current_stats_received.connect( _on_request_user_stats )
	Steam.user_stats_stored.connect( _on_user_stats_stored )
	load_stats()
