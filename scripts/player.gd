class_name Player extends EntityBase

@onready var _walk_effect:GPUParticles2D = $Animations/DustPuff
@onready var _slide_effect:GPUParticles2D = $Animations/SlidePuff
@onready var _dash_effect:GPUParticles2D = $Animations/DashEffect
@onready var _jumpkit_sparks:AnimatedSprite2D = $Animations/JumpkitSparks

const _MAX_WEAPON_SLOTS:int = 8

var _current_weapon:int = 0
var _weapon_slots:Array[ WeaponSlot ]

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
@onready var _ammo_pellet_stacks:Array[ AmmoStack ] = []
@onready var _ammo_heavy_stacks:Array[ AmmoStack ] = []
@onready var _ammo_light_stacks:Array[ AmmoStack ] = []
@onready var _inventory:Inventory = $Inventory

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

# persistant data
var _split_screen:bool = false
var _input_device:int = 0
var _health:float = 100.0
var _rage:float = 0.0
var _flags:int = 0
var _perks:Array = [ null, null, null ]

# Too Fucking Angry to Die
var _perk0_active:bool = false

var _last_mouse_position:Vector2 = Vector2.ZERO
var _input_velocity:Vector2 = Vector2.ZERO
var _hands_used:Hands = Hands.Left
var _left_arm:int = 0
var _right_arm:int = 0
var _last_used_arm:Arm = null

# multiplayer data
var _multiplayer_username:String = ""
var _multiplayer_team:int = 0
var _multiplayer_captures:int = 0
var _multiplayer_returns:int = 0
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

signal on_player_death( attacker: EntityBase, target: EntityBase )

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

func _on_death( attacker: EntityBase, target: EntityBase ) -> void:
	if _perk0_active:
		if _rage > 0.0:
			# make sure they can't abuse it
			_rage = 0.0
			return
	
	emit_signal( "on_player_death", attacker, self )
	_leg_animation.hide()
	_arm_left._animations.hide()
	_arm_right._animations.hide()
	
	_torso_animation.play( "death" )

func _on_damage( attacker: CharacterBody2D, damage: float ) -> void:
	if _flags & PlayerFlags.Dashing:
		return
	
	emit_signal( "_on_player_damage", attacker, damage )
	_health -= damage
	_rage += damage
	
	if _health <= 0.0:
		_on_death( attacker, self )

func save( file: FileAccess ) -> void:
	var section := SaveSection.new()
	
	section.save( "player_" + var_to_str( _input_device ), file )
	section.save_int( "hands_used", _hands_used )
	section.save_float( "health", _health )
	section.save_float( "rage", _rage )
	section.save_vector2( "position", global_position )
	section.save_int( "flags", _flags )
	section.flush()

func load( file: FileAccess ) -> void:
	var section := SaveSection.new()

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
	_idle_timer.start()
	
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
	_idle_timer.start()
	_hud.init( _health, _rage )
	if _drantaril:
		_drantaril.connect( "player_mount_horse", mount_horse )
	set_process_input( _split_screen )
	_last_used_arm = _arm_right
	
	_weapon_slots.resize( _MAX_WEAPON_SLOTS )
	for i in _MAX_WEAPON_SLOTS:
		_weapon_slots[i] = WeaponSlot.new()
		_weapon_slots[i]._index = i

func _physics_process( _delta: float ) -> void:
	if velocity != Vector2.ZERO:
		idle_reset()
	
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
		
		if _last_mouse_position != mousePosition:
			_last_mouse_position = mousePosition
			idle_reset()
		
		_arm_rotation = atan2( mousePosition.y - ( screenSize.x / 2 ), mousePosition.x - ( screenSize.y / 2 ) )
		if mousePosition.x > screenSize.x / 2:
			_draw_rotation = atan2( mousePosition.y - ( screenSize.x / 2 ), mousePosition.x - ( screenSize.y / 2 ) )
			flip_sprite_right()
		elif mousePosition.x < screenSize.x / 2:
			_draw_rotation = -atan2( mousePosition.y - ( screenSize.x / 2 ), ( screenSize.x / 2 ) - mousePosition.x )
			flip_sprite_left()
	
	if Input.is_action_just_pressed( _dash_name ) && can_dash():
		idle_reset()
		_flags |= PlayerFlags.Dashing
		_dash_time.start()
		play_sfx( _dash[ randi_range( 0, _dash.size() - 1 ) ] )
		_dash_effect.show()
		_dash_effect.emitting = true
		_dash_direction = velocity
		_hud._dash_overlay.show()
	
	if Input.is_action_just_pressed( _slide_name ) && !( _flags & PlayerFlags.Sliding ):
		idle_reset()
		_flags |= PlayerFlags.Sliding
		_slide_time.start()
		play_sfx( _slide[ randi_range( 0, _slide.size() - 1 ) ] )
		_slide_effect.emitting = true
		_leg_animation.play( "slide" )
	
	if Input.is_action_just_pressed( "use_weapon_0" ):
		idle_reset()
		if _weapon_slots[ _current_weapon ].is_used():
			_weapon_slots[ _current_weapon ]._weapon.use( _weapon_slots[ _last_used_arm._weapon_slot ]._weapon._last_used_mode )
	
	if Input.is_action_just_pressed( "bullet_time_0" ):
		idle_reset()
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

func _on_idle_animation_timer_timeout() -> void:
	if _idle_animation.is_playing():
		return
	
	_torso_animation.hide()
	_arm_left._animations.hide()
	_arm_right._animations.hide()
	_leg_animation.hide()
	_idle_animation.show()
	_idle_animation.play( "start" )

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
	
	stack.set_type( ammo )
	_inventory.add_to_stack( stack, ammo._data.id, ammo._data.properties.stack_add_amount, ammo._data.properties )
	
	for i in _MAX_WEAPON_SLOTS:
		var slot := _weapon_slots[i]
		if slot.is_used() && slot._weapon._data.properties.ammo_type == ammo._data.properties.type:
			slot._weapon.set_reserve( stack )
			slot._weapon.set_ammo( ammo._data )
	
	_last_used_ammo = ammo

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
			break
	
	_inventory.add( weapon._data.id, 1, weapon._data.properties )
	
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
		if weapon._last_used_mode & WeaponBase.Properties.IsTwoHanded:
			_arm_left._weapon_slot = _current_weapon
			_arm_right._weapon_slot = _current_weapon
			
			# this will automatically override any other modes
			_weapon_slots[ _arm_left._weapon_slot ]._mode = weapon._default_mode
			_weapon_slots[ _arm_right._weapon_slot ]._mode = weapon._default_mode
			return
		
		# update the hand data
		_last_used_arm._weapon_slot = _current_weapon
		_weapon_slots[ _last_used_arm._weapon_slot ]._mode = weapon._properties
