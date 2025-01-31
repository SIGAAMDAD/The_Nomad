class_name MercenaryShotgunner extends MobBase

@export var _vision_renderer:Polygon2D
@export var _alert_color:Color = Color( Color.RED )

@export_category( "Rotation" )
@export var _is_rotating:bool = false
@export var _rotation_speed:float = 0.1
@export var _angle:float = 90.0

@onready var _ray_angle:Vector2 = Vector2( 0, 0 )

@onready var _sight_target:CharacterBody2D = null
@onready var _detection_meter:Line2D = $DetectionMeter

@onready var _think_timer:Timer = $ThinkTimer

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
	
	_action_planner.set_actions( [
		GotoNodeAction.new(),
		IdleAction.new(),
		InvestigateDisturbanceAction.new()
	] );
	
	_world_state.set_state( "has_target", false )
	_world_state.set_state( "alert", false )
	_world_state.set_state( "fear", 0.0 )
	_world_state.set_state( "goto_position", Vector2.ZERO )
	
	_agent.init( self, [
		IdleGoal.new( self ),
		SearchGoal.new( self ),
		GuardGoal.new( self )
	], _action_planner )
	
	add_child( _agent )
	
	match _direction:
		GameConfiguration.DirType.North:
			_angle_dir = Vector2.UP
		GameConfiguration.DirType.East:
			_angle_dir = Vector2.RIGHT
		GameConfiguration.DirType.South:
			_angle_dir = Vector2.DOWN
		GameConfiguration.DirType.West:
			_angle_dir = Vector2.LEFT
	
	_sight = $SightCheck
	_navigation = $NavigationAgent2D
	_animation = $Animations/AnimatedSprite2D
	
	generate_raycasts()

func recalc_sight() -> void:
	if _ray_angle == _angle_dir:
		# no need
		return
	
	_ray_angle == _angle_dir
	for index in range( len( _sight.get_children() ) ):
		var ray := _sight.get_child( index )
		var angle := _angle_between_rays * ( index - _sight.get_child_count() / 2.0 )
		ray.target_position = _angle_dir.rotated( angle ) * _max_view_distance

func generate_raycasts() -> void:
	var rayCount := _angle_cone_of_vision / _angle_between_rays
	
	_ray_angle = _angle_dir
	for index in rayCount:
		var ray := RayCast2D.new()
		var angle := _angle_between_rays * ( index - rayCount / 2.0 )
		ray.target_position = _angle_dir.rotated( angle ) * _max_view_distance
		_sight.add_child( ray )
		ray.enabled = true

func think() -> void:
	# TODO: implement thinker groups
	pass

func _on_navigation_agent_2d_velocity_computed(safe_velocity: Vector2) -> void:
	velocity = safe_velocity
