extends Node2D

@onready var _data:Resource = preload( "res://scenes/effects/debris_cloud.tscn" )

func _on_finish( this: GPUParticles2D ) -> void:
	remove_child( this )

func add_debris( position: Vector2 ) -> void:
	var data = _data.instantiate()
	data.global_position = position
	data.emitting = true
	add_child( data )

func _process( delta: float ) -> void:
	for child in get_children():
		if !child.emitting:
			remove_child( child )
