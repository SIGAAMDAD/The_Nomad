class_name MobData extends Resource

@export var _name:String = ""
@export var _max_health:float = 0.0
@export var _movement_speed:float = 0.0
@export var _max_view_distance:float = 0.0
@export var _view_angle_amount:float = deg_to_rad( 45.0 )
@export var _spritesheet:SpriteFrames = null
@export var _collision_radius:float = 0.0

@export var _valid_actions:PackedStringArray
@export var _valid_goals:PackedStringArray

# the amount of time until the target starts seeing red
@export var _sight_detection_time:float = 10.0

# how fast the mob goes from suspicious to FUCK THEM UP!
@export var _sight_detection_speed:float = 0.1


@export var _max_attack_range:float = 560.0
