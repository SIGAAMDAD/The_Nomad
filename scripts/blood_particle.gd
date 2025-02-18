extends Sprite2D

@onready var _splatter := $BloodSplatter

var _vspeed:float = randf_range( -8.0, 8.0 )
var _hspeed:float = randf_range( -6.0, 6.0 )

func _ready() -> void:
	_vspeed = lerpf( _vspeed, 5.0, 0.02 )
	_hspeed = lerpf( _hspeed, 0.0, 0.02 )
	_splatter.emitting = true
	_splatter.get_child( 0 ).timeout.connect( _on_splatter_timeout )

func _physics_process( _delta: float ) -> void:
	position.y += _vspeed
	position.x += _hspeed

func _on_splatter_timeout() -> void:
	$Timer.start()

func _on_timer_timeout() -> void:
	set_physics_process( false )
	$Timer.queue_free()
