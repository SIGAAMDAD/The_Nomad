class_name MobBase extends CharacterBody2D

@export var _data:MobData = null

@onready var _angle_between_rays:float = deg_to_rad( 5.0 )

@onready var _navigation:NavigationAgent2D = NavigationAgent2D.new()

@export_category( "Detection" )
@export var _sound_bounds:CollisionShape2D = null

@export_category( "Sounds" )
@export var _target_spotted:Array[ AudioStreamPlayer2D ]

@export_category( "Start" )
@export var _direction:GameConfiguration.DirType = GameConfiguration.DirType.North

@onready var _agent:GoapAgent = GoapAgent.new()
@onready var _world_state:WorldStateManager = WorldStateManager.new()
@onready var _action_planner:GoapActionPlanner = GoapActionPlanner.new()
@onready var _sight:Node2D = $SightCheck
@onready var _target:CharacterBody2D = null

@onready var _angle_dir:Vector2 = Vector2( 0, 0 )
@onready var _animations:AnimatedSprite2D = $Animations/AnimatedSprite2D

@onready var _sight_target:CharacterBody2D = null
@onready var _detection_meter:Line2D = $DetectionMeter

@onready var _health:float = 0.0
@onready var _sight_detection_amount:float = 0.0

var _valid_goal_list:Array[ GoapGoal ]
var _valid_action_list:Array[ GoapAction ]

@onready var GOALS:Dictionary = {
	"FindThreats": load( "res://scripts/mobs/mercenary/goals/find_threats.gd" ),
	"EliminateThreats": load( "res://scripts/mobs/mercenary/goals/eliminate_threats.gd" ),
};

@onready var ACTIONS:Dictionary = {
	"Idle": load( "res://scripts/mobs/mercenary/actions/idle.gd" ),
	"InvestigateDisturbance": load( "res://scripts/mobs/mercenary/actions/investigate_disturbance.gd" ),
	"Guard": load( "res://scripts/mobs/mercenary/actions/guard_position.gd" ),
	"ShootTarget": load( "res://scripts/mobs/mercenary/actions/shoot_target.gd" )
};

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func allocate_goals() -> void:
	for goal in _data._valid_goals:
		print( "Added goal " + goal + " to list" )
		_valid_goal_list.push_back( GOALS[ goal ].new( self ) )
	for action in _data._valid_actions:
		_valid_action_list.push_back( ACTIONS[ action ].new() )

func _ready() -> void:
	match _direction:
		GameConfiguration.DirType.North:
			_angle_dir = Vector2.UP
		GameConfiguration.DirType.East:
			_angle_dir = Vector2.RIGHT
		GameConfiguration.DirType.South:
			_angle_dir = Vector2.DOWN
		GameConfiguration.DirType.West:
			_angle_dir = Vector2.LEFT
	
	allocate_goals()
	_action_planner.set_actions( _valid_action_list );
	
	_world_state.set_state( "has_target", false )
	_world_state.set_state( "alert", false )
	_world_state.set_state( "fear", 0.0 )
	_world_state.set_state( "goto_position", Vector2.ZERO )
	
	_agent.init( self, _valid_goal_list, _action_planner )
	
	match _direction:
		GameConfiguration.DirType.North:
			_angle_dir = Vector2.UP
		GameConfiguration.DirType.East:
			_angle_dir = Vector2.RIGHT
		GameConfiguration.DirType.South:
			_angle_dir = Vector2.DOWN
		GameConfiguration.DirType.West:
			_angle_dir = Vector2.LEFT
	
	_animations.sprite_frames = _data._spritesheet
	_animations.play( "idle" )
	
	add_child( _agent )
	generate_raycasts()
	
	_health = _data._max_health


func recalc_sight() -> void:
	_sight.global_transform = global_transform
	for index in range( len( _sight.get_children() ) ):
		var ray := _sight.get_child( index )
		var angle := _angle_between_rays * ( index - _sight.get_child_count() / 2.0 )
		ray.target_position = _angle_dir.rotated( angle ) * _data._max_view_distance

func generate_raycasts() -> void:
	var rayCount := _data._view_angle_amount / _angle_between_rays
	
	_sight.global_transform = global_transform
	for index in rayCount:
		var ray := RayCast2D.new()
		var angle := _angle_between_rays * ( index - rayCount / 2.0 )
		ray.target_position = _angle_dir.rotated( angle ) * _data._max_view_distance
		_sight.add_child( ray )
		ray.enabled = true

func move_along_path( delta: float ) -> void:
	var nextPathPosition = _navigation.get_next_path_position()
	_angle_dir = global_position.direction_to( nextPathPosition ) * _data._movement_speed
	
	_animations.play( "move" )
	
	velocity = _angle_dir
	
	move_and_slide()

func set_target( target: CharacterBody2D ) -> void:
	if _target != null:
		_target_spotted[ randi_range( 0, _target_spotted.size() - 1 ) ].play()
	
	_target = target

func _process( delta: float ) -> void:
	recalc_sight()
	var sightTarget:CharacterBody2D = null
	for ray in _sight.get_children():
		if ray.is_colliding() && ray.get_collider() is Player:
			sightTarget = ray.get_collider()
			break
	
	if sightTarget && _sight_detection_amount < _data._sight_detection_time:
		_detection_meter.default_color = Color( lerpf( 0.05, 1.0, _sight_detection_amount ), 0.0, 0.0, 1.0 )
		_sight_detection_amount += _data._sight_detection_speed * delta
	else:
		_detection_meter.default_color = Color.WHITE
		_sight_detection_amount = 0.0
	
	if _sight_detection_amount >= 1.0 && sightTarget:
		_world_state.set_state( "target", sightTarget )
		_navigation.target_position = sightTarget.global_position
		_target = sightTarget
	
	if velocity == Vector2.ZERO:
		_animations.play( "idle" )
	
	if _angle_dir.x < 0.0:
		_direction = GameConfiguration.DirType.West
		_animations.flip_h = true
	else:
		_direction = GameConfiguration.DirType.East
		_animations.flip_h = false
	
	# set planning state
	_world_state.set_state( "health", _health )
	_world_state.set_state( "is_dying", _health <= 25.0 )
