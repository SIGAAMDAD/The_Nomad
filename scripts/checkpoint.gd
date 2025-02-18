extends Area2D

@onready var _passed_checkpoint:AudioStreamPlayer2D = $PassCheckpoint
@onready var _ambience:AudioStreamPlayer2D = $Ambience

@onready var _light:PointLight2D = $PointLight2D
@onready var _shape:CollisionShape2D = $CollisionShape2D
@onready var _bonfire:AnimatedSprite2D = $Bonfire
@onready var _unlit:Sprite2D = $Unlit

var _passed:bool = false

func free() -> void:
	_shape.queue_free()
	_bonfire.queue_free()
	_unlit.queue_free()
	_light.queue_free()
	_ambience.queue_free()
	_passed_checkpoint.queue_free()

func _ready() -> void:
	pass

func _on_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	# make sure we don't just have a wandering mob triggering a checkpoint
	# or not triggering it twice
	if body is not Player:
		return
	
	_ambience.stop()

	_unlit.show()
	
	_light.queue_free()
	_shape.queue_free()
	_ambience.queue_free()
	_bonfire.queue_free()
	_passed_checkpoint.play()
	_passed = true
	ArchiveSystem.save_game()
