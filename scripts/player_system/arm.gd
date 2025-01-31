class_name Arm extends Node2D

@export var _animations:AnimatedSprite2D = null
@export var _parent:Player = null

@onready var _default_animation:SpriteFrames = null
@onready var _weapon_slot:int = -1

var _flip:bool = false

func _ready() -> void:
	_default_animation = _animations.sprite_frames

func get_animation_set() -> SpriteFrames:
	if _weapon_slot != -1:
		var weapon:WeaponEntity = _parent._inventory._weapon_slots[ _weapon_slot ]._weapon
		if weapon:
			if weapon._last_used_mode & WeaponBase.Properties.IsFirearm:
				return weapon._data._firearm_frames_left if _flip else weapon._data._firearm_frames_right
			elif weapon._last_used_mode & WeaponBase.Properties.IsBlunt:
				return weapon._data._blunt_frames_left if _flip else weapon._data._blunt_frames_right
			elif weapon._last_used_mode & WeaponBase.Properties.IsBladed:
				return weapon._data._bladed_frames_left if _flip else weapon._data._bladed_frames_right
	
	return _default_animation

func _process( _delta: float ) -> void:
	var animation := get_animation_set()
	
	_animations.sprite_frames = animation
	_animations.rotation = _parent._arm_rotation
	if _weapon_slot == -1:
		_animations.flip_h = _flip
		if _parent._input_velocity != Vector2.ZERO:
			_animations.play( "run" )
		else:
			_animations.play( "idle" )
	else:
		var weapon:WeaponEntity = _parent._inventory._weapon_slots[ _weapon_slot ]._weapon
		match weapon._current_state:
			WeaponEntity.WeaponState.Idle:
				_animations.play( "idle" )
			WeaponEntity.WeaponState.Reload:
				_animations.play( "reload" )
			WeaponEntity.WeaponState.Use:
				_animations.play( "use" )
