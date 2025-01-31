class_name HealthPack extends ItemBase

func _on_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not CharacterBody2D:
		return
	
	body.on_heal( 20.0 )
