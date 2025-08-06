class_name HealthBar extends ProgressBar

@onready var _damage_timer: Timer = $DamageTimer
@onready var _show_timer: Timer = $ShowTimer
@onready var _damage_bar: ProgressBar = $DamageBar

@onready var _color_map: Dictionary[ int, Color ] = {
	0: Color.RED,
	1: Color.LIGHT_BLUE,
	2: Color.DARK_ORANGE
}

func SetHealth( health: float ) -> void:
	process_mode = PROCESS_MODE_PAUSABLE
	
	modulate = Color( 1.0, 1.0, 1.0, 1.0 )
	_show_timer.start()
	
	var _prev_health: float = value
	value = min( health, max_value )
	if value <= _prev_health:
		_damage_timer.start()
	else:
		_damage_bar.value = value

func init( health: float ) -> void:
	max_value = health
	value = health
	_damage_bar.max_value = health
	_damage_bar.value = health
	
	process_mode = PROCESS_MODE_DISABLED

	modulate = _color_map[ SettingsData.GetColorblindMode() ]

func _on_show_timer_timeout() -> void:
	var _tweener: Tween = create_tween()
	_tweener.tween_property( self, "modulate", Color( 0.0, 0.0, 0.0, 0.0 ), 1.0 )
	_tweener.connect( "finished", func(): process_mode = PROCESS_MODE_DISABLED )

func _on_damage_timer_timeout() -> void:
	_damage_timer.value = value
