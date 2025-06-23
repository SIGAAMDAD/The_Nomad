class_name RageBar extends ProgressBar

@onready var _show_timer: Timer = $ShowTimer

var Rage: float:
	get:
		return Rage
	set( value ):
		process_mode = PROCESS_MODE_PAUSABLE
		modulate = Color( 1.0, 1.0, 1.0, 1.0)
		_show_timer.start()
		self.value = value

func init( rage: float ) -> void:
	max_value = rage
	value = rage

func _on_show_timer_timeout() -> void:
	var _tweener: Tween = create_tween()
	_tweener.tween_property( self, "modulate", Color( 0.0, 0.0, 0.0, 0.0 ), 1.0 )
	_tweener.connect( "finished", func(): process_mode = PROCESS_MODE_DISABLED )
