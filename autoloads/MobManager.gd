class_name MobLoader extends Node

@onready var _mob_cache:Dictionary

func load_mob_list( path: String, list: Array[ String ] ) -> void:
	var dir := DirAccess.open( path )
	if dir:
		dir.list_dir_begin()
		var fileName := dir.get_next()
		while fileName != "":
			if dir.current_is_dir():
				print( "Found directory: " + fileName )
			else:
				list.push_back( dir.get_current_dir() + "/" + fileName )
			
			fileName = dir.get_next()
	else:
		push_error( "An error occurred when trying to access the path." )

func init() -> void:
	var mobList:Array[ String ]
	load_mob_list( "res://resources/mobs", mobList )
	
	print( "Loading mobs..." )
	for mob in mobList:
		_mob_cache[ mob.get_basename() ] = ResourceLoader.load( mob, "", ResourceLoader.CACHE_MODE_REPLACE )
		if _mob_cache[ mob.get_basename() ]:
			print( "...loaded mob data for \"" + mob + "\"" )
		else:
			push_error( "...couldn't load mob data for \"" + mob + "\"" )
