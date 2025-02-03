extends Node2D

signal on_save_game_start()
signal on_save_game_end()

var _save_slot:int = 0
var _current_chapter:int = 0
var _current_part:int = 0
const _max_save_slots:int = 3

class SaveSlot:
	var _slot:int = 0
	var _sections:Dictionary
	
	func load( slot:int ) -> void:
		print( "Loading save slot ", slot, ", please do not exit the game..." )
		self._slot = slot
		
		var fileName := "user://SLOT_" + var_to_str( slot ) + ".ngd"
		var file := FileAccess.open( fileName, FileAccess.READ );
		
		var sectionCount := file.get_32()
		
		for i in range( 0, sectionCount ):
			var section := SaveSection.new()
			section.load( file )
			_sections[ section._name ] = section
		
		file.close()

var _slots:Array[ JSON ]

func slot_exists( slot: int ) -> bool:
	return FileAccess.file_exists( "user://SLOT_" + var_to_str( slot ) + ".ngd" )

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
	
	var name := "user://SLOT_" + var_to_str( _save_slot ) + ".ngd"
	var file := FileAccess.open( name, FileAccess.WRITE )
	
	var save_nodes := get_tree().get_nodes_in_group( "Archive" )
	for node in save_nodes:
		if node.has_method( "save" ):
			node.save( file )
	
	file.close()
	emit_signal( "on_save_game_end" )

func load_game() -> void:
	print( "Loading game..." )
	var name := "user://SLOT_" + var_to_str( _save_slot ) + ".ngd"
	var file := FileAccess.open( name, FileAccess.READ )
	
	var save_nodes := get_tree().get_nodes_in_group( "Archive" )
	for node in save_nodes:
		if node.has_method( "load" ):
			node.load( file )
	
	file.close()
