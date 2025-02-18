extends ProgressBar

@onready var timer = $Timer
@onready var damage_bar = $DamageBar

var health = 0.0 : set = _set_health

func _set_health( new_health ) -> void:
	var prev_health = health
	health = min( max_value, new_health )
	value = health
	
	if health < prev_health:
		timer.start()
	else:
		damage_bar.value = health

func init( _health ) -> void:
	health = _health
	max_value = health
	value = 80
	damage_bar.max_value = health
	damage_bar.value = health

func _on_timer_timeout() -> void:
	damage_bar.value = health
