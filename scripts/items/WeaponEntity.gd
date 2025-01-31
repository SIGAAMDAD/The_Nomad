class_name WeaponEntity extends Node2D

# TODO: make flying weapons allowed and then make parrying them chunks allowed

@export var _data:WeaponBase = null

@onready var _reserve:ItemStack = null
@onready var _ammo:AmmoEntity = null
@onready var _owner:CharacterBody2D = null
@onready var _animations:AnimatedSprite2D = $Animations/AnimatedSprite2D
@onready var _use_time:Timer = $UseTime
@onready var _use_blunt_sfx:AudioStreamPlayer2D = $UseBlunt
@onready var _use_bladed_sfx:AudioStreamPlayer2D = $UseBladed
@onready var _use_firearm_sfx:AudioStreamPlayer2D = $UseFirearm
@onready var _muzzle_flash:Array[ Sprite2D ]

var _icon_sprite:Sprite2D = Sprite2D.new()
var _area:Area2D = Area2D.new()
var _last_used_mode:WeaponBase.Properties = WeaponBase.Properties.None
var _bullets_left:int = 0

enum WeaponState {
	Idle,
	Use,
	Reload
};

var _current_state:WeaponState = WeaponState.Idle

var _noammo_sound:AudioStreamPlayer2D = null
var _reload_time:Timer = null
var _reload_sfx:AudioStreamPlayer2D = null

var _animations_left:SpriteFrames = null
var _animations_right:SpriteFrames = null

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func _on_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	
	print( "Picking up entity..." )
	_area.set_deferred( "monitoring", false )
	_icon_sprite.hide()
	_owner = body
	reparent( _owner )
	global_position = _owner.global_position
	set_use_mode( _data._default_mode )
	_owner.pickup_weapon( self )

func create_pickup_bounds() -> void:
	print( "Allocating pickup bounds for weapon..." )
	
	var circle := CircleShape2D.new()
	circle.radius = 7.0
	
	var collision := CollisionShape2D.new()
	collision.shape = circle
	
	_area.add_child( collision )
	_area.connect( "body_shape_entered", _on_body_shape_entered )
	
	add_child( _area )

func _ready() -> void:
	if !_data:
		push_error( "Cannot initialize WeaponEntity without a valid WeaponBase (null)" )
		return
	
	_icon_sprite.texture = _data._icon
	_data.init()
	
	if _data._bladed_frames_left:
		print( _data._bladed_frames_left )
	if _data._bladed_frames_right:
		print( _data._bladed_frames_right )
	if _data._firearm_frames_left:
		print( _data._firearm_frames_left.get_animation_names() )
	if _data._firearm_frames_right:
		print( _data._firearm_frames_right.get_animation_names() )
	
	_owner = null
	_use_time.wait_time = _data._use_time
	
	if _data._is_firearm:
		var muzzleFlash0 := Sprite2D.new()
		muzzleFlash0.texture = ResourceLoader.load( "res://textures/env/muzzle/mf0.dds" )
		muzzleFlash0.position.x = 46
		muzzleFlash0.position.y = -2
		muzzleFlash0.scale.x = 0.25
		muzzleFlash0.scale.y = 0.35
		muzzleFlash0.hide()
		_animations.add_child( muzzleFlash0 )
		
		var muzzleFlash1 := Sprite2D.new()
		muzzleFlash1.texture = ResourceLoader.load( "res://textures/env/muzzle/mf1.dds" )
		muzzleFlash1.position.x = 46
		muzzleFlash1.position.y = -2
		muzzleFlash1.scale.x = 0.25
		muzzleFlash1.scale.y = 0.35
		muzzleFlash1.hide()
		_animations.add_child( muzzleFlash1 )
		
		var muzzleFlash2 := Sprite2D.new()
		muzzleFlash2.texture = ResourceLoader.load( "res://textures/env/muzzle/mf2.dds" )
		muzzleFlash2.position.x = 46
		muzzleFlash2.position.y = -2
		muzzleFlash2.scale.x = 0.25
		muzzleFlash2.scale.y = 0.35
		muzzleFlash2.hide()
		_animations.add_child( muzzleFlash2 )
		
		_reload_time = Timer.new()
		_reload_time.wait_time = _data._reload_time
		_reload_time.timeout.connect( _on_reload_time_timeout )
		_noammo_sound = AudioStreamPlayer2D.new()
		_noammo_sound.stream = ResourceLoader.load( "res://sounds/weapons/noammo.wav" )
		
		_reload_sfx = AudioStreamPlayer2D.new()
		_reload_sfx.stream = ResourceLoader.load( "res://sounds/weapons/adb_reload.ogg" )
		
		add_child( _noammo_sound )
		add_child( _reload_sfx )
		add_child( _reload_time )
	
	_use_blunt_sfx.stream = _data._use_blunt_sfx
	_use_bladed_sfx.stream = _data._use_bladed_sfx
	_use_firearm_sfx.stream = _data._use_firearm_sfx
	_use_time.timeout.connect( _on_use_time_timeout )
	
	create_pickup_bounds()
	add_child( _icon_sprite )

func set_use_mode( weaponMode: int ) -> void:
	_last_used_mode = weaponMode
	
	if weaponMode & WeaponBase.Properties.IsFirearm:
		_animations_left = _data._firearm_frames_left
		_animations_right = _data._firearm_frames_right
	elif weaponMode & WeaponBase.Properties.IsBladed:
		_animations_left = _data._bladed_frames_left
		_animations_right = _data._bladed_frames_right
	elif weaponMode & WeaponBase.Properties.IsBlunt:
		_animations_left = _data._blunt_frames_left
		_animations_right = _data._blunt_frames_right

func use_blunt( damage: float, weaponMode: int ) -> float:
	var angle = _owner._arms_right_animation.rotation
	
	play_sfx( _use_blunt_sfx )
	
	_use_time.start()
	
	var ray := RayCast2D.new()
	ray.target_position.x = _data._blunt_range * cos( angle )
	ray.target_position.y = _data._blunt_range * sin( angle )
	
	if ray.is_colliding() && ray.get_collider() is CharacterBody2D:
		ray.get_collider().on_damage( damage )
	
	return damage

func set_ammo( ammo: AmmoEntity ) -> void:
	_ammo = ammo

func set_reserve( stack: ItemStack ) -> void:
	print( "Setting ammo reserve to ", stack._item_type )
	_reserve = stack

func reload() -> bool:
	if !_reserve:
		return false
	if !_reserve.num_items() && !_bullets_left:
		# no more ammo
		print( "cannot reload weapon..." )
		return false
	
	_bullets_left = _reserve.remove_items( _data._magsize )
	_reload_time.start()
	_current_state = WeaponState.Reload
	play_sfx( _reload_sfx )
	return true

func use_firearm( damage: float, weaponMode: int ) -> float:
	if !_ammo || _bullets_left < 0:
		play_sfx( _noammo_sound )
		reload()
		return 0.0
	
	if !_bullets_left && !reload():
		return 0.0
	
	_current_state = WeaponState.Use
	_use_time.start()
	_bullets_left -= 1
	play_sfx( _use_firearm_sfx )
	
	return damage

func use( weaponMode: int ) -> float:
	match _current_state:
		WeaponState.Use, WeaponState.Reload:
			return 0.0
	
	set_use_mode( weaponMode )
	
	if _last_used_mode & WeaponBase.Properties.IsFirearm:
		return use_firearm( 0.0, weaponMode )
	elif _last_used_mode & WeaponBase.Properties.IsBlunt:
		return 0.0
	elif _last_used_mode & WeaponBase.Properties.IsBladed:
		return 0.0
	
	return 0.0

func _on_use_time_timeout() -> void:
	if _last_used_mode & WeaponBase.Properties.IsFirearm:
		reload()
		print( "reloading" )
	else:
		_current_state = WeaponState.Idle

func _on_reload_time_timeout() -> void:
	_current_state = WeaponState.Idle
