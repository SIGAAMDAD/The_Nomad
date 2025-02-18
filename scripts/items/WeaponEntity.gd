class_name WeaponEntity extends Node2D

# TODO: make flying weapons allowed and then make parrying them chunks allowed

@export var _data:ItemDefinition = null

@onready var _bullet_shell := preload( "res://scenes/effects/bullet_shell.tscn" )
@onready var _dust_cloud := preload( "res://scenes/effects/debris_cloud.tscn" )

@export var _bullet_shell_sfx:Array[ AudioStream ] = []
@export var _shotgun_shell_sfx:Array[ AudioStream ] = []

@onready var _raycast := $RayCast2D
@onready var _bullets := load( "res://scenes/Items/bullet.tscn" )
@onready var _reserve:AmmoStack = null
@onready var _ammo:ItemDefinition = null
@onready var _animations:AnimatedSprite2D = $Animations/AnimatedSprite2D
@onready var _use_time:Timer = $UseTime
@onready var _use_blunt_sfx:AudioStreamPlayer2D = $UseBlunt
@onready var _use_bladed_sfx:AudioStreamPlayer2D = $UseBladed
@onready var _use_firearm_sfx:AudioStreamPlayer2D = $UseFirearm
@onready var _muzzle_flash:Array[ Sprite2D ]
@onready var _owner:Player = null
@onready var _icon_sprite:Sprite2D = $Icon

@onready var _muzzle_flashes:Array = [
	$MuzzleFlashes/Sprite2D,
	$MuzzleFlashes/Sprite2D2,
	$MuzzleFlashes/Sprite2D3,
	$MuzzleFlashes/Sprite2D4
];

var _area:Area2D = Area2D.new()
var _last_used_mode:WeaponBase.Properties = WeaponBase.Properties.None
var _bullets_left:int = 0
var _current_muzzle_flash:Sprite2D = null
var _equipped:bool = false

enum WeaponState {
	Idle,
	Use,
	Reload,
	
	# magazine fed specific states
	Empty,
	
	Invalid = -1
};

var _current_state:WeaponState = WeaponState.Idle

var _noammo_sound:AudioStreamPlayer2D = null
var _reload_time:Timer = null
var _reload_sfx:AudioStreamPlayer2D = null

var _animations_left:SpriteFrames = null
var _animations_right:SpriteFrames = null

var _properties:WeaponBase.Properties = WeaponBase.Properties.None
var _default_mode:WeaponBase.Properties = WeaponBase.Properties.None

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func _on_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	
	print( "Picking up entity..." )
	remove_child( _area )
	remove_child( _icon_sprite )
	
	_area.queue_free()
	_icon_sprite.queue_free()
	
	_owner = body
	reparent( _owner )
	global_position = _owner.global_position
	set_use_mode( _default_mode )
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


func init_properties() -> void:
	if _data.properties.is_onehanded:
		_properties |= WeaponBase.Properties.IsOneHanded
	if _data.properties.is_twohanded:
		_properties |= WeaponBase.Properties.IsTwoHanded
	if _data.properties.is_bladed:
		_properties |= WeaponBase.Properties.IsBladed
	if _data.properties.is_blunt:
		_properties |= WeaponBase.Properties.IsBlunt
	if _data.properties.is_firearm:
		_properties |= WeaponBase.Properties.IsFirearm
	
	if _data.properties.default_is_onehanded:
		_default_mode |= WeaponBase.Properties.IsOneHanded
	if _data.properties.default_is_twohanded:
		_default_mode |= WeaponBase.Properties.IsTwoHanded
	if _data.properties.default_is_bladed:
		_default_mode |= WeaponBase.Properties.IsBladed
	if _data.properties.default_is_blunt:
		_default_mode |= WeaponBase.Properties.IsBlunt
	if _data.properties.default_is_firearm:
		_default_mode |= WeaponBase.Properties.IsFirearm

func set_equipped_state( equipped: bool ) -> void:
	if !equipped:
		_reload_time.stop()
		_reload_sfx.stop()
		pass

func _ready() -> void:
	if !_data:
		push_error( "Cannot initialize WeaponEntity without a valid WeaponBase (null)" )
		return
	
	_icon_sprite.texture = _data.icon
	init_properties()
	
	_owner = null
	_use_time.wait_time = _data.properties.use_time
	
	if _data.properties.is_firearm:
		var muzzleFlash0 := Sprite2D.new()
		muzzleFlash0.texture = ResourceLoader.load( "res://textures/env/muzzle/mf0.dds" )
		muzzleFlash0.position.x = 46
		muzzleFlash0.position.y = -2
		muzzleFlash0.scale.x = 0.25
		muzzleFlash0.scale.y = 0.35
		muzzleFlash0.hide()
		_animations.add_child( muzzleFlash0 )
		_muzzle_flash.push_back( muzzleFlash0 )
		
		var muzzleFlash1 := Sprite2D.new()
		muzzleFlash1.texture = ResourceLoader.load( "res://textures/env/muzzle/mf1.dds" )
		muzzleFlash1.position.x = 46
		muzzleFlash1.position.y = -2
		muzzleFlash1.scale.x = 0.25
		muzzleFlash1.scale.y = 0.35
		muzzleFlash1.hide()
		_animations.add_child( muzzleFlash1 )
		_muzzle_flash.push_back( muzzleFlash1 )
		
		var muzzleFlash2 := Sprite2D.new()
		muzzleFlash2.texture = ResourceLoader.load( "res://textures/env/muzzle/mf2.dds" )
		muzzleFlash2.position.x = 46
		muzzleFlash2.position.y = -2
		muzzleFlash2.scale.x = 0.25
		muzzleFlash2.scale.y = 0.35
		muzzleFlash2.hide()
		_animations.add_child( muzzleFlash2 )
		_muzzle_flash.push_back( muzzleFlash2 )
		
		_reload_time = Timer.new()
		_reload_time.one_shot = true
		_reload_time.wait_time = _data.properties.reload_time
		_reload_time.timeout.connect( _on_reload_time_timeout )
		_noammo_sound = AudioStreamPlayer2D.new()
		_noammo_sound.stream = ResourceLoader.load( "res://sounds/weapons/noammo.wav" )
		
		_reload_sfx = AudioStreamPlayer2D.new()
		_reload_sfx.stream = _data.properties.reload_sfx
		
		add_child( _noammo_sound )
		add_child( _reload_sfx )
		add_child( _reload_time )
	
	if _data.properties.is_blunt:
		_use_blunt_sfx.stream = _data.properties.use_blunt
	if _data.properties.is_bladed:
		_use_bladed_sfx.stream = _data.properties.use_bladed
	if _data.properties.is_firearm:
		_use_firearm_sfx.stream = _data.properties.use_firearm
	
	_use_time.timeout.connect( _on_use_time_timeout )
	
	create_pickup_bounds()
	
	add_child( _icon_sprite )

#func _draw():
#	if _ammo and SettingsData._draw_aim_line:
#		if _ammo.properties.range:
#			draw_line( global_position, global_position + Vector2( _ammo.properties.range ), Color.RED )

func set_use_mode( weaponMode: int ) -> void:
	_last_used_mode = weaponMode
	
	if weaponMode & WeaponBase.Properties.IsFirearm:
		_animations_left = _data.properties.firearm_frames_left
		_animations_right = _data.properties.firearm_frames_right
	elif weaponMode & WeaponBase.Properties.IsBladed:
		_animations_left = _data.properties.bladed_frames_left
		_animations_right = _data.properties.bladed_frames_right
	elif weaponMode & WeaponBase.Properties.IsBlunt:
		_animations_left = _data.properties.blunt_frames_left
		_animations_right = _data.properties.blunt_frames_right

func use_blunt( damage: float, weaponMode: int ) -> float:
	var angle = _owner._arms_right_animation.rotation
	
	play_sfx( _use_blunt_sfx )
	
	_use_time.start()
	
	var ray := RayCast2D.new()
	ray.target_position.x = _data.properties.blunt_range * cos( angle )
	ray.target_position.y = _data.properties.blunt_range * sin( angle )
	
	if ray.is_colliding() && ray.get_collider() is CharacterBody2D:
		ray.get_collider().on_damage( damage )
	
	return damage

func set_ammo( ammo: ItemDefinition ) -> void:
	_ammo = ammo

func set_reserve( stack: AmmoStack ) -> void:
	_reserve = stack
	if !_bullets_left:
		# force a reload
		reload()

func spawn_shells() -> void:
	var bulletShell := _bullet_shell.instantiate()
	bulletShell.global_position = global_position
	get_tree().get_current_scene().add_child( bulletShell )
	
	match _data.properties.ammo_type:
		AmmoBase.Type.Light, AmmoBase.Type.Heavy:
			bulletShell._grounded_sfx.stream = _bullet_shell_sfx[ randi_range( 0, _bullet_shell_sfx.size() - 1 ) ]
		AmmoBase.Type.Pellets:
			bulletShell._grounded_sfx.stream = _shotgun_shell_sfx[ randi_range( 0, _shotgun_shell_sfx.size() - 1 ) ]
	
	bulletShell.texture = _ammo.properties.casing_icon

func reload() -> bool:
	if !_reserve:
		return false
	if !_reserve.amount && !_bullets_left:
		# no more ammo
		if _data.properties.magazine_type == WeaponBase.MagazineType.Cycle:
			_current_state = WeaponState.Empty
		else:
			_current_state = WeaponState.Idle
		
		print( "cannot reload weapon..." )
		return false
	
	if _last_used_mode & WeaponBase.Properties.IsOneHanded:
		_owner._last_used_arm = _owner.get_weapon_hand( self )
		_owner._hands_used = Player.Hands.Both
	
	if _data.properties.magazine_type == WeaponBase.MagazineType.Breech && _ammo:
		# ejecting shells
		for i in range( _data.properties.magsize ):
			spawn_shells()
	
	_reload_time.start()
	_current_state = WeaponState.Reload
	play_sfx( _reload_sfx )
	return true

func use_firearm( damage: float, weaponMode: int ) -> float:
	if !_ammo || !_bullets_left:
		play_sfx( _noammo_sound )
		return 0.0
	
	_current_state = WeaponState.Use
	_use_time.start()
	
	match _data.properties.firemode:
		WeaponBase.FireMode.Single:
			_bullets_left -= 1
		WeaponBase.FireMode.Burst:
			_bullets_left -= 2
	
	# bullets work like those in Halo 3.
	# start as a hitscan, then if we don't get a hit after 75% of the distance, turn it into a projectile
	
	_current_muzzle_flash = _muzzle_flashes[ randi_range( 0, _muzzle_flashes.size() - 1 ) ]
	_current_muzzle_flash.show()
	_current_muzzle_flash.rotation = _raycast.rotation
	
	if _owner._arm_left._animations.flip_h:
		_current_muzzle_flash.offset.x = -160
	else:
		_current_muzzle_flash.offset.x = 160
	
	_current_muzzle_flash.flip_h = _owner._arm_left._animations.flip_h
	
	if _data.properties.magazine_type == WeaponBase.MagazineType.Cycle:
		# ejecting shells
		spawn_shells()
	
	play_sfx( _use_firearm_sfx )
	
	if _raycast.is_colliding():
		var collision = _raycast.get_collider()
		if collision is Player && GameConfiguration._game_mode == GameConfiguration.GameMode.Multiplayer:
			SteamNetwork.rpc_on_client( collision._multiplayer_id, _owner, "_on_damage", [ damage ] )
		elif collision is MobBase:
			collision.on_damage( _owner, damage )
		
		var debris := _dust_cloud.instantiate()
		get_tree().get_current_scene().add_child( debris )
		var point:Vector2 = _raycast.get_collision_point()
		debris.create( 0.0, global_position, point )
	
	return damage

func _process( _delta: float ) -> void:
	if !_ammo:
		return
	
	_raycast.global_rotation = _owner._arm_rotation
	_raycast.target_position.x = _ammo.properties.range

func use( weaponMode: int ) -> float:
	if Engine.time_scale == 0.0:
		return 0.0
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
		_current_muzzle_flash.hide()
		if !_bullets_left:
			reload()
			return
	
	_current_state = WeaponState.Idle

func _on_reload_time_timeout() -> void:
	_bullets_left = _reserve.remove_items( _data.properties.magsize )
	if _last_used_mode & WeaponBase.Properties.IsOneHanded:
		match _owner._last_used_arm:
			_owner._arm_left:
				_owner._hands_used = Player.Hands.Left
			_owner._arm_right:
				_owner._hands_used = Player.Hands.Right
	
	_current_state = WeaponState.Idle
