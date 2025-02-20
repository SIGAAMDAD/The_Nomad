extends Node

class MapData:
	var _name:String = ""
	var _filename:String = ""
	var _modes:Array[ String ] = []
	
	func _init( map: MultiplayerMap ) -> void:
		_name = map._name
		_filename = map._filename

@onready var _map_cache:Array[ MapData ]

func load_map_list( path: String, list: Array[ String ] ) -> void:
	var dir := DirAccess.open( path )
	if dir:
		dir.list_dir_begin()
		var fileName := dir.get_next()
		while fileName != "":
			if dir.current_is_dir():
				print( "Found directory: ", fileName )
			else:
				list.push_back( dir.get_current_dir() + "/" + fileName )
			
			fileName = dir.get_next()
	else:
		push_error( "An error occurred when trying to access the path." )

func init() -> void:
	var mapList:Array[ String ]
	load_map_list( "res://resources/multiplayer_maps", mapList )
	
	print( "Loading maps..." )
	for map in mapList:
		_map_cache.push_back( MapData.new( ResourceLoader.load( map, "", ResourceLoader.CACHE_MODE_REPLACE ) ) )
		if _map_cache.back():
			print( "...loaded map data for \"" + map + "\"" )
		else:
			push_error( "...couldn't load map data for \"" + map + "\"" )
