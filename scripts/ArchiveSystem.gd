extends Node2D

signal on_save_game_start()
signal on_save_game_end()
signal load_game_start()
signal load_game_end()

const _SAVE_GAME_DATA_HEADER := "NGD_DATA!"
const _MAX_SAVE_SLOTS:int = 3

var _save_slot:int = 0
var _current_chapter:int = 0
var _current_part:int = 0

func slot_exists( slot: int ) -> bool:
	return FileAccess.file_exists( "user://SaveData/SLOT_" + var_to_str( str ) + "/GameData.ngd" )

func set_slot( slot: int ) -> void:
	_save_slot = slot

func compress( text: String ) -> Array:
	var gzip := StreamPeerGZIP.new()
	gzip.start_compression()
	gzip.put_data( text.to_utf8_buffer() )
	gzip.finish()
	return gzip.get_data( gzip.get_available_bytes() )

func save_game() -> void:
	emit_signal( "on_save_game_start" )
	print( "Saving game..." )
	
	DirAccess.make_dir_recursive_absolute( "user://SaveData/SLOT_" + var_to_str( _save_slot ) )
	
	var name := "user://SaveData/SLOT_" + var_to_str( _save_slot ) + "/GameData.ngd"
	var file := FileAccess.open( name, FileAccess.WRITE )
	
	file.store_buffer( var_to_bytes( _SAVE_GAME_DATA_HEADER ) )
	file.store_pascal_string( ProjectSettings.get_setting( "application/config/version" ) )
	
	var save_nodes := get_tree().get_nodes_in_group( "Archive" )
	for node in save_nodes:
		if node.has_method( "save" ):
			node.save( file )
	
	file.close()
	
	SteamManager.SaveCloudFile( "SaveData/SLOT_" + var_to_str( _save_slot ) + "/GameData.ngd" )
	emit_signal( "on_save_game_end" )

func load_game() -> void:
	print( "Loading game..." )
	var name := "user://SaveData/SLOT_" + var_to_str( _save_slot ) + "/GameData.ngd"
	var file := FileAccess.open( name, FileAccess.READ )
	if !file:
		return
	
	ProjectSettings.get_setting( "application/config/version" )
	var header := var_to_str( file.get_buffer( _SAVE_GAME_DATA_HEADER.length() ) )
	if header != _SAVE_GAME_DATA_HEADER:
		push_error( "Saved game data (slot %s) doesn't have the correct header data, refusing load" % _save_slot )
		return
	
	var version := file.get_pascal_string()
	if version != ProjectSettings.get_setting( "application/config/version" ):
		push_error( "Saved game data (slot %s) doesn't have the correct version, refusing load" % _save_slot )
		return
	
	var save_nodes := get_tree().get_nodes_in_group( "Archive" )
	for node in save_nodes:
		if node.has_method( "load" ):
			node.load( file )
	
	file.close()
