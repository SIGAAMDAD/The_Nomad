class_name Player extends EntityBase

const _MAX_WEAPON_SLOTS:int = 8

@onready var _walk_effect:GPUParticles2D = $Animations/DustPuff
@onready var _slide_effect:GPUParticles2D = $Animations/SlidePuff
@onready var _dash_effect:GPUParticles2D = $Animations/DashEffect
@onready var _jumpkit_sparks:AnimatedSprite2D = $Animations/JumpkitSparks

@onready var _damage_camera_shake:CameraShake = $Camera2D/CameraShake
@onready var _blood_splatter = preload( "res://scenes/effects/blood_splatter.tscn" )

@onready var _switch_to_keyboard:GUIDEAction = preload( "res://resources/binds/actions/gamepad/switch_to_keyboard.tres" )
@onready var _switch_to_gamepad:GUIDEAction = preload( "res://resources/binds/actions/keyboard/switch_to_gamepad.tres" )
@onready var _keyboard_input_mapping:GUIDEMappingContext = preload( "res://resources/binds/binds_keyboard.tres" )
@onready var _gamepad_input_mapping:GUIDEMappingContext = preload( "res://resources/binds/binds_gamepad.tres" )

@onready var _move_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/move_player0.tres" )
@onready var _dash_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/dash_player0.tres" )
@onready var _slide_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/slide_player0.tres" )
@onready var _use_weapon_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/use_weapon_player0.tres" )
@onready var _next_weapon_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/next_weapon_player0.tres" )
@onready var _prev_weapon_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/prev_weapon_player0.tres" )
@onready var _switch_weapon_mode_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/switch_weapon_mode_player0.tres" )
@onready var _bullet_time_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/bullet_time_player0.tres" )
@onready var _arm_angle_action_gamepad:GUIDEAction = load( "res://resources/binds/actions/gamepad/arm_angle.tres" )

@onready var _move_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/move_player0.tres" )
@onready var _dash_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/dash_player0.tres" )
@onready var _slide_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/slide_player0.tres" )
@onready var _use_weapon_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/use_weapon_player0.tres" )
@onready var _next_weapon_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/next_weapon_player0.tres" )
@onready var _prev_weapon_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/prev_weapon_player0.tres" )
@onready var _switch_weapon_mode_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/switch_weapon_mode_player0.tres" )
@onready var _bullet_time_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/bullet_time_player0.tres" )
@onready var _open_inventory_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/open_inventory.tres" )
@onready var _demon_eye_action_keyboard:GUIDEAction = load( "res://resources/binds/actions/keyboard/demon_eye_player0.tres" )

@onready var _move_action:GUIDEAction = null
@onready var _dash_action:GUIDEAction = null
@onready var _slide_action:GUIDEAction = null
@onready var _use_weapon_action:GUIDEAction = null
@onready var _next_weapon_action:GUIDEAction = null
@onready var _prev_weapon_action:GUIDEAction = null
@onready var _switch_weapon_mode_action:GUIDEAction = null
@onready var _bullet_time_action:GUIDEAction = null
@onready var _open_inventory_action:GUIDEAction = null
@onready var _demon_eye_action:GUIDEAction = null
@onready var _arm_angle_action:GUIDEAction = _arm_angle_action_gamepad

@onready var _idle_timer:Timer = $IdleAnimationTimer
@onready var _idle_animation:AnimatedSprite2D = $Animations/Idle

@onready var _leg_animation:AnimatedSprite2D = $Animations/Legs
@onready var _torso_animation:AnimatedSprite2D = $Animations/Torso
@onready var _animations:Node2D = $Animations
@onready var _camera_shake:Camera2D = $Camera2D
@onready var _arm_left:Arm = $ArmLeft
@onready var _arm_right:Arm = $ArmRight
@onready var _hud:CanvasLayer = $HeadsUpDisplay
@onready var _collision:CollisionShape2D = $TorsoCollision2D
@onready var _dash_direction:Vector2 = Vector2.ZERO
@onready var _dash_cooldown:Timer = $Timers/DashCooldownTime
@onready var _dash_time:Timer = $Timers/DashTime
@onready var _slide_time:Timer = $Timers/SlideTime

@onready var _drantaril:CharacterBody2D = null

@onready var _last_used_ammo:AmmoEntity = null
@onready var _stacks:Array[ ItemStack ] = []
@onready var _ammo_pellet_stacks:Array[ AmmoStack ] = []
@onready var _ammo_heavy_stacks:Array[ AmmoStack ] = []
@onready var _ammo_light_stacks:Array[ AmmoStack ] = []
@onready var _inventory:Inventory = $Inventory

@onready var _demon_eye_sfx:AudioStreamPlayer2D = $SoundEffects/DemonEye
@onready var _change_weapon_sfx:AudioStreamPlayer2D = $SoundEffects/ChangeWeapon

@onready var _pain_sfx:Array[ AudioStreamPlayer2D ] = [
	$SoundEffects/Pain0,
	$SoundEffects/Pain1,
	$SoundEffects/Pain2
]

@onready var _die_sfx:Array[ AudioStreamPlayer2D ] = [
	$SoundEffects/Die0,
	$SoundEffects/Die1,
	$SoundEffects/Die2
]

@onready var _death_sfx:Array[ AudioStreamPlayer2D ] = [
	$SoundEffects/DieSound0,
	$SoundEffects/DieSound1,
	$SoundEffects/DieSound2,
]

@onready var _move_gravel:Array[ AudioStreamPlayer2D ] = [
	$SoundEffects/MoveGravel0,
	$SoundEffects/MoveGravel1,
	$SoundEffects/MoveGravel2,
	$SoundEffects/MoveGravel3
]

@onready var _dash:Array[ AudioStreamPlayer2D ] = [
	$SoundEffects/Dash0,
	$SoundEffects/Dash1
]

@onready var _slide:Array[ AudioStreamPlayer2D ] = [
	$SoundEffects/Slide0,
	$SoundEffects/Slide1
]

@onready var _slowmo_begin:AudioStreamPlayer2D = $SoundEffects/SlowMoBegin
@onready var _slowmo_end:AudioStreamPlayer2D = $SoundEffects/SlowMoEnd

enum Hands {
	Left,
	Right,
	Both
};

enum PlayerFlags {
	Sliding			= 0x0001,
	Crouching		= 0x0002,
	BulletTime		= 0x0004,
	Dashing			= 0x0008,
	DemonRage		= 0x0010,
	UsedMana		= 0x0020,
	DemonSight		= 0x0040,
	OnHorse			= 0x0080,
	IdleAnimation	= 0x1000,
};

# persistent data
var _split_screen:bool = false
var _input_device:int = 0
var _health:float = 80.0
var _rage:float = 0.0
var _flags:int = 0
var _hellbreaks:int = 0
var _perks:Array = [ null, null, null ]
var _current_weapon:int = 0
var _weapon_slots:Array[ WeaponSlot ]

# Too Fucking Angry to Die
var _perk0_active:bool = false

var _last_mouse_position:Vector2 = Vector2.ZERO
var _input_velocity:Vector2 = Vector2.ZERO
var _hands_used:Hands = Hands.Left
var _left_arm:int = 0
var _right_arm:int = 0
var _last_used_arm:Arm = _arm_right

var _level_stats:LevelStats = LevelStats.new()

# multiplayer data
var _multiplayer_id:int = 0
var _multiplayer_username:String = ""
var _multiplayer_team:MultiplayerData.Team = null
var _multiplayer_flag_captures:int = 0
var _multiplayer_flag_returns:int = 0
var _multiplayer_hill_time:int = 0
var _multiplayer_kills:int = 0
var _multiplayer_deaths:int = 0

var _dash_overheat_amount:float = 0.0
var _movement:Vector2 = Vector2.ZERO
var _frame_damage:float = 0.0
var _damage_mult:float = 0.0

var _arm_rotation:float = 0.0
var _draw_rotation:float = 0.0

var _move_left_name:String = "move_left_0"
var _move_right_name:String = "move_right_0"
var _move_up_name:String = "move_up_0"
var _move_down_name:String = "move_down_0"
var _dash_name:String = "dash_0"
var _slide_name:String = "slide_0"
var _useweapon_name:String = "useweapon_0"
var _bullet_time_name:String = "bullet_time_0"

const _ACCEL = 1500
const _FRICTION = 1400
const _MAX_SPEED = 400.0
const _JUMP_VELOCITY = -400.0

signal respawn()
signal death( attacker: EntityBase, target: EntityBase )

func get_weapon_hand( weapon: WeaponEntity ) -> Arm:
	if _weapon_slots[ _arm_left._weapon_slot ]._weapon == weapon:
		return _arm_left
	elif _weapon_slots[ _arm_right._weapon_slot ]._weapon == weapon:
		return _arm_right
	
	# not equipped
	return null

func idle_reset() -> void:
	_idle_timer.start()
	_idle_animation.hide()
	_idle_animation.stop()
	
	_leg_animation.show()
	_torso_animation.show()
	_arm_right._animations.show()
	_arm_left._animations.show()

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func free() -> void:
	_torso_animation.queue_free()
	_arm_left._animations.queue_free()
	_arm_right._animations.queue_free()
	_arm_left.queue_free()
	_arm_right.queue_free()
	
	_idle_timer.queue_free()
	_idle_animation.queue_free()
	_inventory.queue_free()
	_hud.queue_free()
	
	queue_free()

func _on_death( attacker: EntityBase, target: EntityBase ) -> void:
	if SettingsData._hellbreaker:
		get_tree().get_current_scene().toggle_hellbreaker()
	
	if _perk0_active:
		if _rage > 0.0:
			# make sure they can't abuse it
			_rage = 0.0
			return
	
	death.emit( attacker, self )
	_leg_animation.hide()
	_arm_left._animations.hide()
	_arm_right._animations.hide()
	
	_torso_animation.play( "death" )

func _on_damage( attacker: CharacterBody2D, damage: float ) -> void:
	if _flags & PlayerFlags.Dashing:
		return
	
	on_damage.emit( attacker, damage )
	_health -= damage
	_rage += damage
	
	_damage_camera_shake.shake( 2.0, 0.5, 4 )
	
	var blood := _blood_splatter.instantiate()
	add_child( blood )
	
	if _health <= 0.0:
		_on_death( attacker, self )
	else:
		play_sfx( _pain_sfx[ randi_range( 0, _pain_sfx.size() - 1 ) ] )

func save( file: FileAccess ) -> void:
	file.store_float( global_position.x )
	file.store_float( global_position.y )
	
	file.store_float( _health )
	file.store_float( _rage )
	file.store_32( _flags )
	file.store_32( _hellbreaks )
	
	file.store_32( _level_stats._collateral_amount )
	file.store_32( _level_stats._death_count )
	file.store_32( _level_stats._kill_count )
	
	file.store_8( _arm_left._weapon_slot )
	file.store_8( _arm_right._weapon_slot )
	
	for slot in _weapon_slots:
		file.store_32( slot._mode )
		file.store_8( slot._weapon != null )
		if slot._weapon:
			file.store_pascal_string( slot._weapon._data.id )
	
	file.store_32( _ammo_heavy_stacks.size() )
	for stack in _ammo_heavy_stacks:
		file.store_pascal_string( stack._ammo_type.id )
		file.store_32( stack.amount )
	
	file.store_32( _ammo_light_stacks.size() )
	for stack in _ammo_light_stacks:
		file.store_pascal_string( stack._ammo_type.id )
		file.store_32( stack.amount )
	
	file.store_32( _ammo_pellet_stacks.size() )
	for stack in _ammo_pellet_stacks:
		file.store_pascal_string( stack._ammo_type.id )
		file.store_32( stack.amount )

func load( file: FileAccess ) -> void:
	_health = file.get_float()
	_rage = file.get_float()
	_flags = file.get_32()
	_hellbreaks = file.get_32()
	
	_arm_left._weapon_slot = file.get_8()
	_arm_right._weapon_slot = file.get_8()
	
	for slot in _weapon_slots:
		slot._mode = file.get_32()
		if file.get_8():
			slot._weapon = WeaponEntity.new()
			add_child( slot._weapon )
			slot._weapon._data = _inventory.database.get_item( file.get_pascal_string() )

func setup_split_screen( input_index: int ) -> void:
	print( "Setting up split-screen input for ", input_index )
	
	if Input.get_connected_joypads().size() > 0:
		_split_screen = true
	
	_input_device = input_index
	_move_left_name = "move_left_" + var_to_str( _input_device )
	_move_right_name = "move_right_" + var_to_str( _input_device )
	_move_down_name = "move_down_" + var_to_str( _input_device )
	_move_up_name = "move_up_" + var_to_str( _input_device )
	_useweapon_name = "use_weapon_" + var_to_str( _input_device )
	_bullet_time_name = "bullet_time_" + var_to_str( _input_device )

func switch_weapon_wielding() -> void:
	var src:Arm = null
	var dst:Arm = null
	
	match _hands_used:
		Hands.Left:
			src = _arm_left
			dst = _arm_right
		Hands.Right:
			src = _arm_right
			dst = _arm_left
	
	if src._weapon_slot == -1:
		# nothing in the source hand, deny
		return
	
	_last_used_arm = dst
	
	var srcWeapon:WeaponEntity = _inventory._weapon_slots[ src._weapon_slot ]._weapon
	if srcWeapon._last_used_mode & WeaponBase.Properties.IsTwoHanded && !( srcWeapon._last_used_mode & WeaponBase.Properties.IsOneHanded ):
		# cannot change hands, no one-handing allowed
		return
	
	# check if the destination hand has something in it, if true, then swap
	if dst._weapon_slot != -1:
		var tmp := dst._weapon_slot
		
		dst._weapon_slot = src._weapon_slot
		src._weapon_slot = tmp
	else:
		# if we have nothing in the destination hand, then just clear the source hand
		var tmp := src._weapon_slot
		
		src._weapon_slot = -1
		dst._weapon_slot = tmp

func switch_weapon_mode() -> void:
	# weapon mode order (default)
	# blade -> blunt -> firearm
	
	var slot:WeaponSlot = null
	
	match _hands_used:
		Hands.Left:
			var index := _arm_left._weapon_slot
			if index == int( -1 ):
				return
			slot = _inventory._weapon_slots[ index ]
		Hands.Right:
			var index := _arm_right._weapon_slot
			if index == int( -1 ):
				return
			slot = _inventory._weapon_slots[ index ]
		Hands.Both:
			slot = _inventory._weapon_slots[ _inventory._current_weapon ]
	
	var mode := slot._mode

func switch_used_hand() -> void:
	pass

func flip_sprite_right() -> void:
	_leg_animation.flip_h = false
	_torso_animation.flip_h = false
	_arm_left._flip = false
	_arm_right._flip = false
	if _drantaril:
		_drantaril._animations.flip_h = false

func flip_sprite_left() -> void:
	_leg_animation.flip_h = true
	_torso_animation.flip_h = true
	_arm_left._flip = true
	_arm_right._flip = true
	if _drantaril:
		_drantaril._animations.flip_h = true

#func _input( event: InputEvent ) -> void:
#	_idle_timer.start()
#	
#	if event is not InputEventJoypadMotion:
#		return
#	elif event.get_axis() != JOY_AXIS_RIGHT_X && event.get_axis() != JOY_AXIS_RIGHT_Y:
#		return
#	
#	_arm_rotation = deg_to_rad( event.axis_value )
#	_draw_rotation = event.axis_value

func mount_horse() -> void:
	print( "Mounting horse..." )
	_flags |= PlayerFlags.OnHorse
	_leg_animation.play( "horse" )
	_arm_left._animations.play( "idle" )
	_arm_right._animations.play( "idle" )
	_animations.add_child( _drantaril )
	_animations.move_child( _drantaril, 1 )
	_drantaril.mount( self )
	
	# TODO: make some dedicated player horse sprites
	remove_child( _collision )
	_collision.shape = _drantaril._collision_body.shape
	add_child( _collision )
#	_drantaril.global_transform = global_transform

func set_player_position( position ) -> void:
	global_position = position

func _on_dash() -> void:
	idle_reset()
	_flags |= PlayerFlags.Dashing
	_dash_time.start()
	play_sfx( _dash[ randi_range( 0, _dash.size() - 1 ) ] )
	_dash_effect.show()
	_dash_effect.emitting = true
	_dash_direction = velocity
	_hud._dash_overlay.show()

func _on_slide() -> void:
	if _flags & PlayerFlags.Sliding:
		return
	
	idle_reset()
	_flags |= PlayerFlags.Sliding
	_slide_time.start()
	play_sfx( _slide[ randi_range( 0, _slide.size() - 1 ) ] )
	_slide_effect.emitting = true
	_leg_animation.play( "slide" )

func _on_use_weapon() -> void:
	idle_reset()
	if _weapon_slots[ _current_weapon ].is_used():
		_frame_damage += _weapon_slots[ _current_weapon ]._weapon.use( _weapon_slots[ _last_used_arm._weapon_slot ]._weapon._last_used_mode )

func _on_bullet_time() -> void:
	idle_reset()
	if _flags & PlayerFlags.BulletTime:
		exit_bullet_time()
	else:
		_flags |= PlayerFlags.BulletTime
		play_sfx( _slowmo_begin )
		AudioServer.playback_speed_scale = 0.50
		Engine.time_scale = 0.010

func _on_arm_angle_changed() -> void:
	_arm_rotation = _arm_angle_action.value_axis_1d
	_draw_rotation = _arm_rotation

func _on_next_weapon() -> void:
	var index := 0 if _current_weapon == _MAX_WEAPON_SLOTS - 1 else _current_weapon + 1
	while index < _MAX_WEAPON_SLOTS:
		if _weapon_slots[ index ].is_used():
			break
		index += 1
	
	if index == _MAX_WEAPON_SLOTS:
		index = -1
	
	play_sfx( _change_weapon_sfx )
	_current_weapon = index
	
	# adjust arm state
	if index != -1:
		var weapon:WeaponEntity = null
		var otherArm:Arm = null
		match _last_used_arm:
			_arm_left:
				otherArm = _arm_right
				_hands_used = Hands.Left
			_arm_right:
				otherArm = _arm_left
				_hands_used = Hands.Right
		
		weapon = _weapon_slots[ index ]._weapon
		if weapon._last_used_mode & WeaponBase.Properties.IsTwoHanded:
			otherArm._weapon_slot = -1
			_hands_used = Hands.Both
		
		_hud.set_weapon( weapon )
	else:
		_hud.set_weapon( null )
	
	_last_used_arm.set_weapon( _current_weapon )

func _on_prev_weapon() -> void:
	var index := _MAX_WEAPON_SLOTS - 1 if _current_weapon <= 0 else _current_weapon - 1
	while index != -1:
		if _weapon_slots[ index ].is_used():
			break
		index -= 1
	
	play_sfx( _change_weapon_sfx )
	
	if index != -1:
		var weapon:WeaponEntity = null
		var otherArm:Arm = null
		match _last_used_arm:
			_arm_left:
				otherArm = _arm_right
				_hands_used = Hands.Left
			_arm_right:
				otherArm = _arm_left
				_hands_used = Hands.Right
		
		weapon = _weapon_slots[ index ]._weapon
		if weapon._last_used_mode & WeaponBase.Properties.IsTwoHanded:
			otherArm._weapon_slot = -1
			_hands_used = Hands.Both
		
		_hud.set_weapon( weapon )
	else:
		_hud.set_weapon( null )
	
	_current_weapon = index
	_last_used_arm.set_weapon( _current_weapon )

func _on_demon_eye_on() -> void:
	if !_demon_eye_sfx.playing:
		play_sfx( _demon_eye_sfx )
	_hud._demon_eye_overlay.show()
	GameConfiguration._demon_eye_active = true

func _on_demon_eye_off() -> void:
	_demon_eye_sfx.stop()
	_hud._demon_eye_overlay.hide()
	GameConfiguration._demon_eye_active = false

func _switch_input_mode( inputContext: GUIDEMappingContext ) -> void:
	GUIDE.enable_mapping_context( inputContext )
	
	if inputContext == _keyboard_input_mapping:
		_move_action = _move_action_keyboard
		_dash_action = _dash_action_keyboard
		_slide_action = _slide_action_keyboard
		_bullet_time_action = _bullet_time_action_keyboard
		_prev_weapon_action = _prev_weapon_action_keyboard
		_next_weapon_action = _next_weapon_action_keyboard
		_switch_weapon_mode_action = _switch_weapon_mode_action_keyboard
		_use_weapon_action = _use_weapon_action_keyboard
		_demon_eye_action = _demon_eye_action_keyboard
	else:
		_move_action = _move_action_gamepad
		_dash_action = _dash_action_gamepad
		_slide_action = _slide_action_gamepad
		_bullet_time_action = _bullet_time_action_gamepad
		_prev_weapon_action = _prev_weapon_action_gamepad
		_next_weapon_action = _next_weapon_action_gamepad
		_switch_weapon_mode_action = _switch_weapon_mode_action_gamepad
#		_use_weapon_action = _use_weapon_action_gamepad

func _cmd_set_health( health: float ) -> void:
	_health = health

func _ready() -> void:
	Console.add_command( "set_player_health", _cmd_set_health, [ _health ], 1 )
	
	#
	# initialize input context
	#
	_switch_to_keyboard.triggered.connect( _switch_input_mode.bind( _keyboard_input_mapping ) )
	_switch_to_gamepad.triggered.connect( _switch_input_mode.bind( _gamepad_input_mapping ) )
	
	if _split_screen:
		_arm_angle_action.triggered.connect( _on_arm_angle_changed )
		_switch_input_mode( _gamepad_input_mapping )
	else:
		_switch_input_mode( _keyboard_input_mapping )
	
	_dash_action.triggered.connect( _on_dash )
	_slide_action.triggered.connect( _on_slide )
	_bullet_time_action.triggered.connect( _on_bullet_time )
	_use_weapon_action.triggered.connect( _on_use_weapon )
	_switch_weapon_mode_action.triggered.connect( switch_weapon_mode )
	_next_weapon_action.triggered.connect( _on_next_weapon )
	_prev_weapon_action.triggered.connect( _on_prev_weapon )
	_open_inventory_action_keyboard.triggered.connect( _hud._on_show_inventory )
#	_demon_eye_action.triggered.connect( _on_demon_eye_on )
#	_demon_eye_action.completed.connect( _on_demon_eye_off )
	
	if GameConfiguration._game_mode == GameConfiguration.GameMode.Multiplayer:
		_multiplayer_username = SteamManager._steam_username
		_multiplayer_id = SteamManager._steam_id
		
		SteamNetwork.register_rpc( self, "set_player_position", SteamNetwork.PERMISSION.CLIENT_ALL )
		SteamNetwork.register_rset( self, "_health", SteamNetwork.PERMISSION.CLIENT_ALL )
		SteamNetwork.register_rset( self, "_rage", SteamNetwork.PERMISSION.CLIENT_ALL )
		SteamNetwork.register_rset( self, "global_position", SteamNetwork.PERMISSION.CLIENT_ALL )
		SteamNetwork.register_rpc( self, "on_death", SteamNetwork.PERMISSION.CLIENT_ALL )
	
	_idle_timer.start()
	_hud.init( _health, _rage )
	if _drantaril:
		_drantaril.connect( "player_mount_horse", mount_horse )
	_last_used_arm = _arm_right
	
	_weapon_slots.resize( _MAX_WEAPON_SLOTS )
	for i in _MAX_WEAPON_SLOTS:
		_weapon_slots[i] = WeaponSlot.new()
		_weapon_slots[i]._index = i

func _physics_process( _delta: float ) -> void:
	if Console.is_visible():
		return
	
	if velocity != Vector2.ZERO:
		idle_reset()
	
	_damage_camera_shake.shake( 20.0, 0.5, 4 )
	
	var speed := _MAX_SPEED
	if _flags & PlayerFlags.Dashing:
		speed += 1800
	if _flags & PlayerFlags.Sliding:
		speed += 400
	
	_input_velocity = _move_action.value_axis_2d
	if _input_velocity != Vector2.ZERO:
		velocity = velocity.move_toward( _input_velocity * speed, _delta * _ACCEL )
	else:
		velocity = velocity.move_toward( Vector2.ZERO, _delta * _FRICTION )
	
	if _input_velocity != Vector2.ZERO:
		if !( _flags & PlayerFlags.Sliding ) && !( _flags & PlayerFlags.OnHorse ):
			_leg_animation.play( "run" )
			_walk_effect.emitting = true
		elif _flags & PlayerFlags.OnHorse:
			_drantaril._animations.play( "run" )
	else:
		if !( _flags & PlayerFlags.OnHorse ):
			_leg_animation.play( "idle" )
			_walk_effect.emitting = false
			_slide_effect.emitting = false
		elif _drantaril:
			if velocity != Vector2.ZERO:
				_drantaril._animations.play( "stop" )
			else:
				_drantaril._animations.play( "idle" )
	
	move_and_slide()
	
	if _flags & PlayerFlags.OnHorse:
		_drantaril.velocity = velocity
		_drantaril.move_and_slide()

func can_dash() -> bool:
	return !( _flags & PlayerFlags.Dashing ) && _dash_cooldown.time_left == 0.0

func exit_bullet_time() -> void:
	_flags &= ~PlayerFlags.BulletTime
	play_sfx( _slowmo_end )
	_hud._reflex_overlay.hide()
	AudioServer.playback_speed_scale = 1.0
	Engine.time_scale = 1.0

func check_status( delta: float ) -> void:
#	if _rage < 100.0:
#		_hud.show_rage_bar()
	if _frame_damage > 0.0:
		_rage += _frame_damage * delta
		_frame_damage = 0.0
		_flags |= PlayerFlags.UsedMana
	
#	_frame_damage = 0.0 # BUG: you can just stand still and nothing would kill you
#	if _health < 100.0 && _rage > 0.0:
#		_health += 0.075 * delta
#		_rage -= 0.5 * delta
#		
#		_flags |= PlayerFlags.UsedMana
	
	if _flags & PlayerFlags.BulletTime:
		if _rage <= 0.0:
			exit_bullet_time()
		_rage -= 0.5 * delta
	
	if _rage > 100.0:
		_rage = 100.0
	if _rage < 0.0:
		_rage = 0.0
	
	_hud._health_bar._set_health( _health )
	_hud._rage_bar.value = _rage

func _process( delta: float ) -> void:
	if Console.is_visible():
		return
	
	# the voices!
	if _demon_eye_sfx.playing:
		_demon_eye_sfx.global_position = global_position
	
	if _split_screen:
		var direction := Input.get_axis( _move_left_name, _move_right_name )
		
		if direction > 0.0:
			flip_sprite_right()
		elif direction < 0.0:
			flip_sprite_left()
	else:
		var screenSize := DisplayServer.window_get_size()
		var mousePosition := Vector2( 0, 0 )
		if SettingsData._window_mode >= SettingsManager.WindowMode.Fullscreen:
			mousePosition = DisplayServer.mouse_get_position()
		else:
			mousePosition = get_viewport().get_mouse_position()
		
		if _last_mouse_position != mousePosition:
			_last_mouse_position = mousePosition
			idle_reset()
		
		_arm_rotation = atan2( mousePosition.y - ( screenSize.y / 2 ), mousePosition.x - ( screenSize.x / 2 ) )
#		_arm_rotation = get_local_mouse_position().angle()
		if mousePosition.x > screenSize.x / 2:
			flip_sprite_right()
		elif mousePosition.x < screenSize.x / 2:
			flip_sprite_left()
	
	check_status( delta )
	
	var back:Arm = null
	var front:Arm = null
	if _torso_animation.flip_h:
		back = _arm_right
		front = _arm_left
	else:
		back = _arm_left
		front = _arm_right
	
	_arm_left._animations.hide()
	_arm_right._animations.hide()
	
	if _hands_used == Hands.Both:
		front = _last_used_arm
		back._animations.hide()
	else:
		back._animations.show()
	
	if _flags & PlayerFlags.OnHorse:
		_animations.move_child( front._animations, 4 )
		_animations.move_child( back._animations, 0 )
	else:
		_animations.move_child( back._animations, 0 )
		_animations.move_child( front._animations, 3 )
	
	front._animations.show()

func _on_legs_animation_looped() -> void:
	if velocity != Vector2.ZERO && !( _flags & PlayerFlags.OnHorse ):
		play_sfx( _move_gravel[ randi_range( 0, _move_gravel.size() - 1 ) ] )

func _on_dash_time_timeout() -> void:
	_hud._dash_overlay.hide()
	_dash_effect.emitting = false
	_dash_effect.hide()
	_flags &= ~PlayerFlags.Dashing
	if _leg_animation.flip_h:
		_jumpkit_sparks.flip_h = false
		_jumpkit_sparks.offset.x = 225.0
	else:
		_jumpkit_sparks.flip_h = true
		_jumpkit_sparks.offset.x = 0.0
	
	_jumpkit_sparks.show()
	_jumpkit_sparks.play( "default" )

func _on_slide_time_timeout() -> void:
	_slide_effect.emitting = false
	_flags &= ~PlayerFlags.Sliding

func _on_jumpkit_sparks_animation_finished() -> void:
	_jumpkit_sparks.hide()

func _on_dash_cooldown_time_timeout() -> void:
	pass # Replace with function body.

func _on_idle_animation_timer_timeout() -> void:
	if _idle_animation.is_playing():
		return
	
	_torso_animation.hide()
	_arm_left._animations.hide()
	_arm_right._animations.hide()
	_leg_animation.hide()
	_idle_animation.show()
	_idle_animation.play( "start" )
	SteamAchievements.activate_achievement( SteamAchievements.AchievementID.Smoke_Break )

func _on_idle_animation_finished() -> void:
	_idle_animation.play( "loop" )

func on_pickup_ammo( ammo: AmmoEntity ) -> void:
	var stack:AmmoStack = null
	
	match ammo._data.properties[ "type" ]:
		AmmoBase.Type.Light:
			for index in _ammo_light_stacks:
				if ammo._data == index._ammo_type:
					stack = index
					break
			
			stack = AmmoStack.new()
			_ammo_light_stacks.push_back( stack )
		AmmoBase.Type.Heavy:
			for index in _ammo_heavy_stacks:
				if ammo._data == index._ammo_type:
					stack = index
					break
			
			stack = AmmoStack.new()
			_ammo_heavy_stacks.push_back( stack )
		AmmoBase.Type.Pellets:
			for index in _ammo_pellet_stacks:
				if ammo._data == index._ammo_type:
					stack = index
					break
			
			stack = AmmoStack.new()
			_ammo_pellet_stacks.push_back( stack )
	
	stack._player = self
	stack.set_type( ammo )
	stack.add_items( ammo._data.properties.stack_add_amount )
#	_inventory.add( ammo._data.id, ammo._data.properties.stack_add_amount, ammo._data.properties )
	
	for i in _MAX_WEAPON_SLOTS:
		var slot := _weapon_slots[i]
		if slot.is_used() && slot._weapon._data.properties.ammo_type == ammo._data.properties.type:
			slot._weapon.set_reserve( stack )
			slot._weapon.set_ammo( ammo._data )
	
	_last_used_ammo = ammo
	_last_used_arm = _arm_right

func get_equipped_weapon() -> WeaponSlot:
	return _weapon_slots[ _current_weapon ]

func equip_slot( slot: int ) -> void:
	var currentWeapon := slot
	
	var weapon := _weapon_slots[ slot ]._weapon
	if weapon:
		# apply rules of various weapno properties
		if weapon._last_used_mode & WeaponBase.Properties.IsTwoHanded:
			_arm_left._weapon_slot = _current_weapon
			_arm_right._weapon_slot = _current_weapon
			
			# this will automatically override any other modes
			_weapon_slots[ _arm_left._weapon_slot ]._mode = weapon._default_mode
			_weapon_slots[ _arm_right._weapon_slot ]._mode = weapon._default_mode
		
		_weapon_slots[ _last_used_arm._weapon_slot ]._mode = weapon._properties
	else:
		_arm_left._weapon_slot = -1
		_arm_right._weapon_slot = -1
	
	# update hand data
	_last_used_arm._weapon_slot = _current_weapon
	
	_hud.set_weapon( weapon )

func pickup_weapon( weapon: WeaponEntity ) -> void:
	for i in _MAX_WEAPON_SLOTS:
		if !_weapon_slots[i].is_used():
			_weapon_slots[i]._weapon = weapon
			_current_weapon = i
			print( "assigning weapon slot %s..." % i )
			break
	
	_inventory.add( weapon._data.id, 1, weapon._data.properties )
	
	_torso_animation.flip_h = false
	_arm_left._animations.flip_h = false
	_arm_right._animations.flip_h = false
	_leg_animation.flip_h = false
	
	var stack:AmmoStack = null
	match weapon._data.properties.ammo_type:
		AmmoBase.Type.Light:
			if _ammo_light_stacks.size():
				stack = _ammo_light_stacks.back()
				weapon.set_reserve( stack )
				weapon.set_ammo( _ammo_light_stacks.back()._ammo_type )
		AmmoBase.Type.Heavy:
			if _ammo_heavy_stacks.size():
				stack = _ammo_heavy_stacks.back()
				weapon.set_reserve( stack )
				weapon.set_ammo( _ammo_heavy_stacks.back()._ammo_type )
		AmmoBase.Type.Pellets:
			if _ammo_pellet_stacks.size():
				stack = _ammo_pellet_stacks.back()
				weapon.set_reserve( stack )
				weapon.set_ammo( _ammo_pellet_stacks.back()._ammo_type )
	
	if SettingsData._equip_weapon_on_pickup:
		_hud.set_weapon( weapon )
		
		# apply rules of various weapon properties
		if weapon._default_mode & WeaponBase.Properties.IsTwoHanded:
			_arm_left._weapon_slot = _current_weapon
			_arm_right._weapon_slot = _current_weapon
			
			_hands_used = Hands.Both
			_last_used_arm = _arm_right
			
			# this will automatically override any other modes
			_weapon_slots[ _arm_left._weapon_slot ]._mode = weapon._default_mode
			_weapon_slots[ _arm_right._weapon_slot ]._mode = weapon._default_mode
		elif weapon._default_mode & WeaponBase.Properties.IsOneHanded:
			if !_last_used_arm:
				_last_used_arm = _arm_right
			_last_used_arm._weapon_slot = _current_weapon
			
			match _last_used_arm:
				_arm_right:
					_hands_used = Hands.Right
				_arm_left:
					_hands_used = Hands.Left
			
			_weapon_slots[ _last_used_arm._weapon_slot ]._mode = weapon._default_mode
		
		# update the hand data
		_last_used_arm._weapon_slot = _current_weapon
		_weapon_slots[ _last_used_arm._weapon_slot ]._mode = weapon._properties
		weapon._last_used_mode = weapon._default_mode
