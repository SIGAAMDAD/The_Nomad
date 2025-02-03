extends CharacterBody2D

@export var _data:ItemDefinition = null

const _FRICTION = 1400

var _dir:float = 0.0
var _spawn_pos:Vector2 = Vector2.ZERO
var _spawn_angle:float = 0.0
var _z_index:int = 0
var _speed:float = 0.0

func _ready() -> void:
	var ray:RayCast2D = $RayCast2D
	if ray.is_colliding() && ray.get_collider() != Player:
		queue_free()
	
	global_position = _spawn_pos
	global_rotation = _spawn_angle
	z_index = _z_index
	_speed = _data.properties.velocity

func _physics_process( delta: float ) -> void:
	# TODO: have bullets lose velocity/power over range
	if global_position.distance_to( _spawn_pos ) > _data.properties.range:
		queue_free()
	
	velocity = Vector2( _speed, 0.0 ).rotated( _dir )
	move_and_slide()

func _on_area_2d_body_entered( body: Node2D ) -> void:
	if body == Player:
		return
	
	print( "Bullet hit!" )
	queue_free()
