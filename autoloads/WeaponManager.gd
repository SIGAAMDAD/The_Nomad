class_name WeaponLoader extends Node

@onready var _weapon_cache:Dictionary

func load_weapon_list( path: String, list: Array[ String ] ) -> void:
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
	var weaponList:Array[ String ]
	load_weapon_list( "res://resources/weapons", weaponList )
	
	print( "Loading weapons..." )
	for weapon in weaponList:
		_weapon_cache[ weapon.get_basename() ] = ResourceLoader.load( weapon, "", ResourceLoader.CACHE_MODE_REPLACE )
		if _weapon_cache[ weapon.get_basename() ]:
			print( "...loaded weapon data for \"" + weapon + "\"" )
		else:
			push_error( "...couldn't load weapon data for \"" + weapon + "\"" )
