extends Node2D

@export var _move_to:Node2D = null

signal level_end()

func _on_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		pass
	
	level_end.emit()
