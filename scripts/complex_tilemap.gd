class_name ComplexTilemap extends Node2D

@export var _floors:Array[ TileMapFloor ] = []
@export var _exterior:TileMapLayer = null
#@export var _quest:QuestResource = null

func _ready() -> void:
	pass

func _physics_process( delta: float ) -> void:
	pass

func hide_all() -> void:
	print( "hiding all floors..." )
	for floor in _floors:
		floor._floor.hide()
		floor._decor.hide()
	
	_exterior.hide()

func show_all() -> void:
	print( "showing all floors..." )
	for floor in _floors:
		floor._floor.show()
		floor._decor.show()
	
	_exterior.show()
