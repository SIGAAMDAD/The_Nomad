extends CharacterBody2D

signal player_mount_horse( user: CharacterBody2D )

@onready var _user:CharacterBody2D = null
@onready var _collision_body:CollisionShape2D = $CollisionShape2D
@onready var _mount_area:Area2D = $MountArea
@onready var _mount_shape:CollisionShape2D = $MountArea/MountShape
@onready var _animations:AnimatedSprite2D = $Animations/AnimatedSprite2D

@onready var _mount:AudioStreamPlayer2D = $Mount
@onready var _gallop:AudioStreamPlayer2D = $Gallop
@onready var _snorting:Array[ AudioStreamPlayer2D ] = [
	$Snorting0,
	$Snorting1,
	$Snorting2
]

func play_sfx( sfx: AudioStreamPlayer2D ) -> void:
	sfx.global_position = global_position
	sfx.play()

func _ready() -> void:
	set_process( false )

func dismount() -> void:
	_user = null
	play_sfx( _mount ) # TODO: maybe have a different sound effect?

func mount( user: CharacterBody2D ) -> void:
	_user = user
	_animations.play( "idle" )
	set_process( true )
	play_sfx( _mount )

func _process( delta: float ) -> void:
	if velocity == Vector2.ZERO:
		return
	
	if !_gallop.playing:
		play_sfx( _gallop )
		play_sfx( _snorting[ randi_range( 0, _snorting.size() - 1 ) ] )

func _on_mount_area_body_shape_entered( body_rid: RID, body: Node2D, body_shape_index: int, local_shape_index: int ) -> void:
	if body is not Player:
		return
	
	global_position = body.global_position
	_mount_area.set_deferred( "monitoring", false )
	_collision_body.disabled = false
	emit_signal( "player_mount_horse" )

func _on_gallop_finished() -> void:
	if velocity == Vector2.ZERO:
		return
	
	play_sfx( _gallop )

func _on_snort_finished() -> void:
	if velocity == Vector2.ZERO:
		return
	
	play_sfx( _snorting[ randi_range( 0, _snorting.size() - 1 ) ] )
