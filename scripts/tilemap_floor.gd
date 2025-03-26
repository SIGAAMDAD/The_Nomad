class_name TileMapFloor extends Node2D

@export var _floor:TileMapLayer = null
@export var _decor:TileMapLayer = null
@export var _area:Area2D = null

@export var _lower:TileMapFloor = null
@export var _exterior:TileMapLayer = null

func _ready() -> void:
	_area.body_shape_entered.connect( _on_area_2d_body_shape_entered )
	_area.body_shape_exited.connect( _on_area_2d_body_shape_exited )

func _on_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
#	get_parent().hide_all()
	show()
	if _exterior:
		print( "Hiding exterior..." )
		_exterior.hide()
	if _lower:
		_lower.material.set( "shader_parameter/alpha_blend", true )
		_lower.show()

func _on_area_2d_body_shape_exited( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
#	get_parent().show_all()
	if _exterior:
		print( "showing exterior..." )
		_exterior.show()
	if _lower:
		_lower.material.set( "shader_parameter/alpha_blend", false )
		_lower.show()
