class_name Arm extends Node2D

@export var _animations:AnimatedSprite2D = null

@onready var _default_animation:SpriteFrames = null
@onready var _weapon_slot:int = -1

var _parent:Player = null
var _flip:bool = false

func _ready() -> void:
	_parent = get_parent()
	_default_animation = _animations.sprite_frames

func set_weapon( slot: int ) -> void:
	_weapon_slot = slot

func get_animation_set() -> SpriteFrames:
	if _weapon_slot != -1:
		var weapon:WeaponEntity = _parent._weapon_slots[ _weapon_slot ]._weapon
		if weapon:
			if weapon._last_used_mode & WeaponBase.Properties.IsFirearm:
				return weapon._data.properties.firearm_frames_left if _flip else weapon._data.properties.firearm_frames_right
			elif weapon._last_used_mode & WeaponBase.Properties.IsBlunt:
				return weapon._data.properties.blunt_frames_left if _flip else weapon._data.properties.blunt_frames_right
			elif weapon._last_used_mode & WeaponBase.Properties.IsBladed:
				return weapon._data.properties.bladed_frames_left if _flip else weapon._data.properties.bladed_frames_right
	
	return _default_animation

func _process( _delta: float ) -> void:
	var animation := get_animation_set()
	
	_animations.sprite_frames = animation
	
	_animations.global_rotation = _parent._arm_rotation
	_animations.flip_v = _parent._torso_animation.flip_h
	
	if _weapon_slot == -1:
		if _parent._input_velocity != Vector2.ZERO:
			_animations.play( "run" )
		else:
			_animations.play( "idle" )
	else:
		var weapon:WeaponEntity = _parent._weapon_slots[ _weapon_slot ]._weapon
		var animationName:String = ""
		match weapon._current_state:
			WeaponEntity.WeaponState.Idle:
				animationName = "idle"
			WeaponEntity.WeaponState.Reload:
				animationName = "reload"
			WeaponEntity.WeaponState.Use:
				animationName = "use"
			WeaponEntity.WeaponState.Empty:
				animationName = "empty"
		
		if weapon._last_used_mode & WeaponBase.Properties.IsOneHanded:
			if ( self == _parent._arm_left && !_animations.flip_h ) || ( self == _parent._arm_right && _animations.flip_h ):
				animationName += "_flip"
		
		_animations.play( animationName )
