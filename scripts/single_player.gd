extends Node2D

@onready var _player1:Node = $Network/Player1
@onready var _hellbreaker:Node2D = null
@export var _level_data:Node2D = null

func toggle_hellbreaker() -> void:
	_level_data.hide()
	_level_data.set_process( false )
	_level_data.set_process_input( false )
	_level_data.set_process_internal( false )
	_level_data.set_physics_process( false )
	_level_data.set_process_unhandled_input( false )
	
	_hellbreaker.hide()
	_hellbreaker.set_process( true )
	_hellbreaker.set_process_input( true )
	_hellbreaker.set_process_internal( true )
	_hellbreaker.set_physics_process( true )
	_hellbreaker.set_process_unhandled_input( true )
	
	add_child( _hellbreaker )

func free() -> void:
	_player1.queue_free()
	if _hellbreaker:
		_hellbreaker.queue_free()

func _ready() -> void:
	get_tree().set_current_scene( self )
	if Input.get_connected_joypads().size() > 0:
		_player1.setup_split_screen( 0 )
	
	MobSfxCache.cache()
	if SettingsData._hellbreaker:
		_hellbreaker = load( "res://levels/hellbreaker.tscn" ).instantiate()
		
		_hellbreaker.hide()
		_hellbreaker.set_process( false )
		_hellbreaker.set_process_input( false )
		_hellbreaker.set_process_internal( false )
		_hellbreaker.set_physics_process( false )
		_hellbreaker.set_process_unhandled_input( false )
