class_name ComplexTilemap extends Node2D

@export var _floors:Array[ TileMapFloor ] = []
@export var _exterior:TileMapLayer = null
#@export var _quest:QuestResource = null

@onready var _location_area:Area2D = $BuildingArea
var _inside:bool = false

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

func _on_building_area_body_shape_entered(body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int) -> void:
	_inside = true

func _on_building_area_body_shape_exited(body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int) -> void:
	_inside = false
