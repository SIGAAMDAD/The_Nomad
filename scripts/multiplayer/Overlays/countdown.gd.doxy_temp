class_name Countdown extends Label

@onready var _timer: Timer = $Timer

signal countdown_timeout

func update() -> void:
	text = var_to_str( _timer.time_left )

func start_countdown() -> void:
	_timer.start()

func set_time_left( _time: float ) -> void:
	text = var_to_str( _time )

func get_time_left() -> float:
	return _timer.time_left

func _ready() -> void:
	_timer.connect( "timeout", func(): emit_signal( "countdown_timeout" ) ) 