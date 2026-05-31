extends Node2D


func _process(delta: float) -> void:
	$RigidBody2D3/Hand.rotation = get_viewport().get_mouse_position().angle()
