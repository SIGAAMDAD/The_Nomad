extends CanvasLayer

signal transition_finished()

@onready var _color_rect:ColorRect = $ColorRect
@onready var _animation_player:AnimationPlayer = $AnimationPlayer

func _ready() -> void:
	_animation_player.animation_finished.connect( _on_animation_finished )

func _on_animation_finished( animationName: String ) -> void:
	if animationName == "fade_to_black":
		transition_finished.emit()
		_animation_player.play( "fade_to_normal" )
	elif animationName == "fade_to_normal":
		_color_rect.hide()

func transition() -> void:
	_color_rect.show()
	_animation_player.play( "fade_to_black" )
