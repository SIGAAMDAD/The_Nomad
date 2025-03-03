extends Control

class ModData:
	var _name:String = ""

var _mod_cache:Dictionary

func load_mod_list( path: String, list: Array[ String ] ) -> void:
	var dir := DirAccess.open( path )
	if dir:
		dir.list_dir_begin()
		var fileName := dir.get_next()
		while fileName != "":
			if dir.current_is_dir():
				print( "Found directory: " + fileName )
			elif fileName.get_extension() == "pck":
				list.push_back( dir.get_current_dir() + "/" + fileName )
			
			fileName = dir.get_next()
	else:
		push_error( "An error occurred when trying to access the path." )

func _ready() -> void:
	var modList:Array[ String ]
	load_mod_list( "res://mods/", modList )
	
	print( "Loading mods..." )
	for mod in modList:
		var modLoaded := ProjectSettings.load_resource_pack( mod )
		if modLoaded:
			print( "...loaded mod data for \"" + mod + "\"" )
		else:
			push_error( "...couldn't load mod data for \"" + mod + "\"" )
			continue
		
		_mod_cache[ mod.get_basename() ] = ResourceLoader.load( "res://" + mod.get_basename() + "_metadata.tres" )
