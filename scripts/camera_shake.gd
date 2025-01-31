class_name CameraShake extends Camera2D

@export var _random_strength:float = 30.0
@export var _shake_fade:float = 5.0

var rng = RandomNumberGenerator.new()

var _shake_strength:float = 0.0

func apply_shake() -> void:
	_shake_strength = _random_strength

func randomOffset() -> Vector2:
	return Vector2( rng.randf_range( -_shake_strength, _shake_strength ), rng.randf_range( -_shake_strength, _shake_strength ) )

func _process( delta: float ) -> void:
	if _shake_strength > 0.0:
		_shake_strength = lerpf( _shake_strength, 0.0, _shake_fade * delta )
		
		offset = randomOffset()
