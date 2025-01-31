extends GPUParticles2D

func _init( flip_h: bool ) -> void:
#	direction.x = -direction.x if flip_h else direction.x
	scale.x = -scale.x if flip_h else scale.x
	emitting = true

func _on_timer_timeout() -> void:
	queue_free()
