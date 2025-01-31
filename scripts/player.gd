class_name Player extends EntityBase

@onready var _walk_effect:GPUParticles2D = $Animations/DustPuff
@onready var _slide_effect:GPUParticles2D = $Animations/SlidePuff
@onready var _dash_effect:GPUParticles2D = $Animations/DashEffect
@onready var _jumpkit_sparks:AnimatedSprite2D = $Animations/JumpkitSparks

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

@onready var _inventory = $Inventory

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
	Sliding		= 0x0001,
	Crouching	= 0x0002,
	BulletTime	= 0x0004,
	Dashing		= 0x0008,
	DemonRage	= 0x0010,
	UsedMana	= 0x0020,
	DemonSight	= 0x0040,
	OnHorse		= 0x0080,
};

# persistant data
var _split_screen:bool = false
var _input_device:int = 0
var _health:float = 100.0
var _rage:float = 0.0
var _flags:int = 0

var _input_velocity:Vector2 = Vector2.ZERO
var _hands_used:Hands = Hands.Left
var _left_arm:int = 0
var _right_arm:int = 0
var _last_used_arm:Arm = null

var _dash_overheat_amount:float = 0.0
var _movement:Vector2 = Vector2.ZERO
var _frame_damage:float = 0.0
var _damage_mult:float = 0.0

var _arm_rotation:float = 0.0

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

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func pickup_weapon( weapon: WeaponEntity ) -> void:
	_inventory.pickup_weapon( weapon )

func on_pickup_ammo( ammo: AmmoEntity ) -> void:
	_inventory.pickup_ammo( ammo )

func on_death( attacker: CharacterBody2D ) -> void:
	emit_signal( "_on_death", attacker )

func on_damage( attacker: CharacterBody2D, damage: float ) -> void:
	emit_signal( "_on_damage", attacker, damage )
	_health -= damage
	_rage += damage
	
	if _health <= 0.0:
		on_death( attacker )

func save( file: FileAccess ) -> void:
	var section := SaveSection.new()
	
	section.save( "player_" + str( _input_device ), file )
	section.save_float( "health", _health )
	section.save_float( "rage", _rage )
	section.save_float( "position.x", global_position.x )
	section.save_float( "position.y", global_position.y )
	section.save_bool( "is_splitscreen", _split_screen )
	section.save_int( "input_device", _input_device )
	section.flush()
	
	_inventory.save()

func load( file: FileAccess ) -> void:
	var section := SaveSection.new()
	
	section.load( file )
	_health = section.load_float( "health" )
	_rage = section.load_float( "rage" )
	global_position.x = section.load_float( "position.x" )
	global_position.y = section.load_float( "position.y" )
	_split_screen = section.load_bool( "is_splitscreen" )
	_input_device = section.load_int( "input_device" )

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

func _input( event: InputEvent ) -> void:
	if event is not InputEventJoypadMotion:
		return
	elif event.get_axis() != JOY_AXIS_RIGHT_X && event.get_axis() != JOY_AXIS_RIGHT_Y:
		return
	
	_arm_rotation = event.axis_value

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

func _ready() -> void:
	Engine.max_fps = 0
	_hud.init( _health, _rage )
	if _drantaril:
		_drantaril.connect( "player_mount_horse", mount_horse )
	set_process_input( _split_screen )
	_last_used_arm = _arm_right

func _physics_process( _delta: float ) -> void:
	var speed := _MAX_SPEED
	if _flags & PlayerFlags.Dashing:
		speed += 1800
	if _flags & PlayerFlags.Sliding:
		speed += 400
	
	_input_velocity = Input.get_vector( _move_left_name, _move_right_name, _move_up_name, _move_down_name )
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

func _process( delta: float ) -> void:
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
		
		if mousePosition.x > screenSize.x / 2:
			_arm_rotation = atan2( mousePosition.y - ( screenSize.x / 2 ), mousePosition.x - ( screenSize.y / 2 ) )
			flip_sprite_right()
		elif mousePosition.x < screenSize.x / 2:
			_arm_rotation = -atan2( mousePosition.y - ( screenSize.x / 2 ), ( screenSize.x / 2 ) - mousePosition.x )
			flip_sprite_left()
	
	if Input.is_action_just_pressed( _dash_name ) && can_dash():
		_flags |= PlayerFlags.Dashing
		_dash_time.start()
		play_sfx( _dash[ randi_range( 0, _dash.size() - 1 ) ] )
		_dash_effect.show()
		_dash_effect.emitting = true
		_dash_direction = velocity
		_hud._dash_overlay.show()
	
	if Input.is_action_just_pressed( _slide_name ) && !( _flags & PlayerFlags.Sliding ):
		_flags |= PlayerFlags.Sliding
		_slide_time.start()
		play_sfx( _slide[ randi_range( 0, _slide.size() - 1 ) ] )
		_slide_effect.emitting = true
		_leg_animation.play( "slide" )
	
	if Input.is_action_just_pressed( "use_weapon_0" ):
		if _inventory._weapon_slots[ _inventory._current_weapon ].is_used():
			_inventory._weapon_slots[ _inventory._current_weapon ]._weapon.use( _inventory._weapon_slots[ _last_used_arm._weapon_slot ]._weapon._last_used_mode )
	
	if Input.is_action_just_pressed( "bullet_time_0" ):
		if _flags & PlayerFlags.BulletTime:
			_flags &= ~PlayerFlags.BulletTime
			play_sfx( _slowmo_end )
			_hud._reflex_overlay.hide()
			AudioServer.playback_speed_scale = 1.0
			Engine.time_scale = 1.0
		else:
			_flags |= PlayerFlags.BulletTime
			play_sfx( _slowmo_begin )
			AudioServer.playback_speed_scale = 0.50
			Engine.time_scale = 0.10
	
	var back:Arm = null
	var front:Arm = null
	if _torso_animation.flip_h:
		back = _arm_right
		front = _arm_left
	else:
		back = _arm_left
		front = _arm_right
	
	_arm_left.hide()
	_arm_right.hide()
	
	if _hands_used == Hands.Both:
		front = _last_used_arm
		back.hide()
	else:
		back.show()
	
	if _flags & PlayerFlags.OnHorse:
		_animations.move_child( front._animations, 4 )
		_animations.move_child( back._animations, 1 )
	else:
		_animations.move_child( front._animations, 3 )
		_animations.move_child( back._animations, 1 )
	
	front.show()

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
