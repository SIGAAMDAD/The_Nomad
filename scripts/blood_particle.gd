extends Area2D

var _is_colliding:bool = false

var _vspeed:float = randf_range( -5.0, 5.0 )
var _hspeed:float = randf_range( -3.0, 3.0 )
var _blood_acc:float = randf_range( 0.05, 0.1 )

var _do_wobble:bool = false

# lifetime
var _max_count:int = randi_range( 10, 50 )
var _count:int = 0

func _physics_process( _delta: float ) -> void:
	HandleBloodMovement()
	
	if _is_colliding:
		BloodSurface.draw_blood( position )
	
	if position.y > 1000:
		queue_free()

func HandleBloodMovement() -> void:
	if !_is_colliding:
		# airborne
		_do_wobble = false
		_vspeed = lerpf( _vspeed, 5.0, 0.02 )
		_hspeed = lerpf( _hspeed, 0.0, 0.02 )
		visible = true
	else:
		_count += 1
		if _count > _max_count:
			queue_free()
		
		# we make sure blood drop faster than 3, slowly reduce speed
		if _vspeed > 3.0:
			_vspeed = 3.0
		
		_vspeed = lerpf( _vspeed, 0.1, _blood_acc )
		if _hspeed > 0.1 || _hspeed < -0.1:
			_hspeed = lerpf( _hspeed, 0.0, 0.1 )
		else:
			_do_wobble = true
		
		visible = false
	
	if _do_wobble:
		_hspeed += randf_range( -0.01, 0.01 )
		_hspeed = clamp( _hspeed, -0.1, 0.1 )
	
	position.y += _vspeed
	position.x += _hspeed

func _on_body_entered( body: Node2D ) -> void:
	_is_colliding = true

func _on_body_exited( body: Node2D ) -> void:
	_is_colliding = false

func _on_timer_timeout() -> void:
	queue_free()
