extends Sprite2D

var _grounded_sfx:AudioStreamPlayer2D = null

var _vspeed:float = randf_range( -2.75, 2.75 )
var _hspeed:float = randf_range( -2.5, 2.5 )

func _ready() -> void:
	_grounded_sfx = AudioStreamPlayer2D.new()
	_grounded_sfx.finished.connect( _on_sound_finished )
	_grounded_sfx.volume_db = 20.0
	_grounded_sfx.max_distance = 900
	add_child( _grounded_sfx )
	$Timer.timeout.connect( _on_timer_timeout )
	
	_vspeed = lerpf( _vspeed, 5.0, 0.02 )
	_hspeed = lerpf( _hspeed, 0.0, 0.02 )

func _physics_process( _delta: float ) -> void:
	position.y += _vspeed
	position.x += _hspeed

func _on_sound_finished() -> void:
	_grounded_sfx.queue_free()

func _on_timer_timeout() -> void:
	set_physics_process( false )
	_grounded_sfx.global_position = global_position
	_grounded_sfx.play()
	$Timer.queue_free()
