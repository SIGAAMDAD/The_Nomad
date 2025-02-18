class_name DebrisSmoke extends Sprite2D

var _vspeed:float = randf_range( -0.025, 0.025 )
var _hspeed:float = randf_range( -0.001, 0.001 )

func _physics_process( _delta: float ) -> void:
	_vspeed = lerpf( _vspeed, 5.0, 0.002 )
	_hspeed = lerpf( _hspeed, 0.0, 0.0002 )
	
	position.y += _vspeed
	position.x += _hspeed
