extends Node2D

@export var _interior:Node2D = null
@export var _area:Area2D = null

func _ready() -> void:
	_area.body_shape_entered.connect( _on_area_2d_body_shape_entered )

func _on_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	_interior.hide()
