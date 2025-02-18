extends MultiMeshInstance2D

func remove_instance() -> void:
	multimesh.instance_count -= 1

func add_instance( position: Vector2 ) -> void:
	multimesh.instance_count += 1
	multimesh.set_instance_transform_2d( multimesh.instance_count - 1, Transform2D( 0.0, position ) )

func _ready() -> void:
	pass
