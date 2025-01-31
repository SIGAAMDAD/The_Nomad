extends Control

signal on_poem_completed

@onready var _current_timer:int = 0
@onready var TIMERS:Array[ Timer ] = [
	$Label/Timer1,
	$Label2/Timer2,
	$Label3/Timer3,
	$Label4/Timer4
];
@onready var LABELS:Array[ Label ] = [
	$Label,
	$Label2,
	$Label3,
	$Label4,
	$Label5
];

func advance_timer() -> void:
	if _current_timer >= LABELS.size():
		emit_signal( "on_poem_completed" )
		queue_free()
	
	_current_timer += 1
	if _current_timer < TIMERS.size():
		TIMERS[ _current_timer ].start()
	else:
		LABELS[ _current_timer ].show()

func _process( _delta: float ) -> void:
	if Input.is_action_just_pressed( "ui_accept" ):
		TIMERS[ _current_timer ].stop()
		advance_timer()

func _on_timer_1_timeout() -> void:
	advance_timer()

func _on_timer_2_timeout() -> void:
	advance_timer()

func _on_timer_3_timeout() -> void:
	advance_timer()

func _on_timer_4_timeout() -> void:
	advance_timer()

func _on_timer_5_timeout() -> void:
	advance_timer()
