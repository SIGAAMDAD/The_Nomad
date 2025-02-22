extends Node2D

@onready var _player1:Player = $Network/Player1
@onready var _adaptive_soundtrack:Node2D = $AdaptiveSoundtrack
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

func free() -> void:
	_player1.queue_free()
	_adaptive_soundtrack.queue_free()
	if _hellbreaker:
		_hellbreaker.queue_free()

func _ready() -> void:
	get_tree().set_current_scene( self )
	_adaptive_soundtrack.init( _player1 )
	if Input.get_connected_joypads().size() > 0:
		_player1.setup_split_screen( 0 )
	
	if SettingsData._hellbreaker:
		_hellbreaker = load( "res://levels/hellbreaker.tscn" ).instantiate()
		
		_hellbreaker.hide()
		_hellbreaker.set_process( false )
		_hellbreaker.set_process_input( false )
		_hellbreaker.set_process_internal( false )
		_hellbreaker.set_physics_process( false )
		_hellbreaker.set_process_unhandled_input( false )
		add_child( _hellbreaker )
