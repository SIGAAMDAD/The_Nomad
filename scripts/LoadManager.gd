extends Node

signal progress_changed( progrees: float )
signal load_done

@onready var _load_screen:PackedScene = preload( "res://scenes/menus/loading_screen.tscn" )
var _loaded_resource:PackedScene
var _scene_path:String
var _new_loading_screen:Node
var _progress:Array = []

var use_sub_threads:bool = true

func load_scene( path: String ) -> void:
	_scene_path = path
	
	if !_new_loading_screen:
		_new_loading_screen = _load_screen.instantiate()
	
	get_tree().get_root().add_child( _new_loading_screen )
	
	self.progress_changed.connect( _new_loading_screen._update_progress_bar )
	self.load_done.connect( _new_loading_screen._start_outro_animation )
	
	await Signal( _new_loading_screen, "loading_screen_has_full_coverage" )
	
	start_load()

func start_load() -> void:
	var state := ResourceLoader.load_threaded_request( _scene_path, "", use_sub_threads )
	if state == OK:
		set_process( true )

func _process( _delta: float ):
	var loadStatus := ResourceLoader.load_threaded_get_status( _scene_path, _progress )
	match loadStatus:
		0, 2: #? THREAD_LOAD_INVALID_RESORUCE, THREAD_LOAD_FAILED
			print( "Couldn't load level." )
			set_process( false )
			return
		1: #? THREAD_LOAD_IN_PROGRESS
			emit_signal( "progress_changed", _progress[0] )
		3: #? THREAD_LOAD_LOADED
			print( "Loaded scene." )
			_loaded_resource = ResourceLoader.load_threaded_get( _scene_path )
			emit_signal( "progress_changed", 1.0 )
			emit_signal( "load_done" )
			
			get_tree().get_root().remove_child( _new_loading_screen )
			get_tree().change_scene_to_packed( _loaded_resource )
			_loaded_resource = null
