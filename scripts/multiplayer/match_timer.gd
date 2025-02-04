extends Label

@onready var _timer:Timer = $Timer

func set_match_time( time: int ) -> void:
	_timer.one_shot = true
	_timer.wait_time = time

func _process( _delta: float ) -> void:
	text = str( _timer.time_left )
