class_name WeaponBase extends Resource

enum FireMode {
	Single,
	Burst,
	Automatic,
	
	Invalid = -1
};

enum Properties {
	IsOneHanded			= 0b01000000,
	IsTwoHanded			= 0b00100000,
	IsBladed			= 0b00000001,
	IsBlunt				= 0b00000010,
	IsFirearm			= 0b00001000,
	
	OneHandedBlade		= IsOneHanded | IsBladed,
	OneHandedBlunt		= IsOneHanded | IsBlunt,
	OneHandedFirearm	= IsOneHanded | IsFirearm,

	TwoHandedBlade		= IsTwoHanded | IsBladed,
	TwoHandedBlunt		= IsTwoHanded | IsBlunt,
	TwoHandedFirearm	= IsTwoHanded | IsFirearm,

	SpawnsObject		= 0b10000000,

	None				= 0b00000000 # here simply for the hell of it
};

@export var _one_handed:bool = false
@export var _two_handed:bool = false
@export var _is_bladed:bool = false
@export var _is_blunt:bool = false
@export var _is_firearm:bool = false
@export var _magsize:int = 0
@export var _firemode:FireMode = FireMode.Invalid
@export var _icon:ImageTexture = null
@export var _blunt_range:float = 0.0
@export var _bladed_range:float = 0.0
@export var _firearm_frames_right:SpriteFrames = null
@export var _firearm_frames_left:SpriteFrames = null
@export var _blunt_frames_right:SpriteFrames = null
@export var _blunt_frames_left:SpriteFrames = null
@export var _bladed_frames_left:SpriteFrames = null
@export var _bladed_frames_right:SpriteFrames = null
@export var _use_blunt_sfx:AudioStream = null
@export var _use_bladed_sfx:AudioStream = null
@export var _use_firearm_sfx:AudioStream = null
@export var _reload_sfx:AudioStream = null
@export var _pickup_sfx:AudioStream = null
@export var _use_time:float = 0.0
@export var _reload_time:float = 0.0
@export var _damage:float = 0.0
@export var _ammo_type:AmmoBase.Type = 0

@export var _default_one_handed:bool = false
@export var _default_two_handed:bool = false
@export var _default_is_bladed:bool = false
@export var _default_is_blunt:bool = false
@export var _default_is_firearm:bool = false

var _properties:Properties = Properties.None
var _default_mode:Properties = Properties.None
var _animations:AnimatedSprite2D = AnimatedSprite2D.new()

func init() -> void:
	if _one_handed:
		_properties |= Properties.IsOneHanded
	if _two_handed:
		_properties |= Properties.IsTwoHanded
	if _is_bladed:
		_properties |= Properties.IsBladed
	if _is_blunt:
		_properties |= Properties.IsBlunt
	if _is_firearm:
		_properties |= Properties.IsFirearm
	
	if _default_one_handed:
		_default_mode |= Properties.IsOneHanded
	if _default_two_handed:
		_default_mode |= Properties.IsTwoHanded
	if _default_is_bladed:
		_default_mode |= Properties.IsBladed
	if _default_is_blunt:
		_default_mode |= Properties.IsBlunt
	if _default_is_firearm:
		_default_mode |= Properties.IsFirearm
