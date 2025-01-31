extends Node

var StringTable:Array

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var file = FileAccess.open( "res://string_table.csv", FileAccess.READ )
	
	while !file.eof_reached():
		var string = file.get_csv_line()
		StringTable.append( string )

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
