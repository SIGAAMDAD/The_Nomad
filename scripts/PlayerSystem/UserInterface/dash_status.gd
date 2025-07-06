class_name DashStatus extends ProgressBar

@onready var _show_timer: Timer = $ShowTimer

var DashBurnout: float:
	get:
		return DashBurnout
	set( value ):
		process_mode = PROCESS_MODE_PAUSABLE
		modulate = Color( 1.0, 1.0, 1.0, 1.0 )
		_show_timer.start()
		create_tween().tween_property( self, "value", value, 0.25 )

func init( max_burnout: float ) -> void:
	max_value = max_burnout
	value = 0.0

func _on_show_timer_timeout() -> void:
	var _tweener: Tween = create_tween()
	_tweener.tween_property( self, "modulate", Color( 0.0, 0.0, 0.0, 0.0 ), 1.0 )
	_tweener.connect( "finished", func(): process_mode = PROCESS_MODE_DISABLED )
