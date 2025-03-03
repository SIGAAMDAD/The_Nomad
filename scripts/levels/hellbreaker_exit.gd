extends Node2D

@onready var _exit_sound:AudioStreamPlayer2D = $ExitSound
@onready var _use_animation:AnimatedSprite2D = $UseAnimation
@onready var _default_animation:AnimatedSprite2D = $Idle
@onready var _area:InteractableArea2D = $InteractableArea2D

func _ready() -> void:
	_exit_sound.global_position = global_position
	_use_animation.hide()
	_default_animation.show()

func _on_area_2d_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	
	_exit_sound.play()
	_default_animation.hide()
	_use_animation.show()
	_use_animation.play( "use" )
	
	_area.queue_free()

func _on_use_animation_animation_finished() -> void:
	_use_animation.hide()
	_default_animation.show()
	_default_animation.play( "dead" )
