extends Node2D

@export var _respawn_time:float = 0.0
@export var _item:ItemDefinition = null
@export var _is_weapon:bool = false
@export var _is_ammo:bool = false

var _timer:Timer = Timer.new()
var _weapon:WeaponEntity = null
var _ammo:AmmoEntity = null

func pickup( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	
	if _is_weapon:
		_weapon._on_body_shape_entered( body_rid, body, body_shape_index, local_shape_index )
	elif _is_ammo:
		_ammo._on_pickup_area_2d_body_shape_entered( body_rid, body, body_shape_index, local_shape_index )
	
	_weapon = null
	_ammo = null
	
	_timer.one_shot = true
	_timer.start()

func _on_timer_timeout() -> void:
	if _is_weapon:
		_weapon = WeaponEntity.new()
		_weapon._data = _item
		add_child( _weapon )
		_weapon._area.area_shape_entered.connect( pickup )
	elif _is_ammo:
		_ammo = AmmoEntity.new()
		_ammo._data = _item
		add_child( _ammo )
		_ammo._pickup_area.area_shape_entered.connect( pickup )

func _ready() -> void:
	_timer.timeout.connect( _on_timer_timeout )
	
	_on_timer_timeout()
