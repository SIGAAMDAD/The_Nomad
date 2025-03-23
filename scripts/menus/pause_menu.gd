extends Control

@onready var _confirm_exit_dlg:ConfirmationDialog = $ConfirmExit
@onready var _confirm_quit_dlg:ConfirmationDialog = $ConfirmQuit
@onready var _confirm_dlg_overlay:ColorRect = $ColorRect2

signal leave_lobby()

func _process( _delta: float ) -> void:
	if Input.is_action_just_pressed( "ui_exit" ):
		pause_menu()

func pause_menu() -> void:
	if GameConfiguration._paused:
		hide()
	else:
		show()
		Engine.time_scale = 0
	
	GameConfiguration._paused = !GameConfiguration._paused;

func _on_resume_button_pressed() -> void:
	pause_menu()

func _on_exit_to_main_menu_button_pressed() -> void:
	_confirm_exit_dlg.show()
	_confirm_dlg_overlay.show()

func _on_exit_game_button_pressed() -> void:
	_confirm_quit_dlg.show()
	_confirm_dlg_overlay.show()

func _on_confirm_exit_confirmed() -> void:
	GameConfiguration._paused = false
	
	Engine.time_scale = 1
	GameConfiguration.LoadedLevel.UnloadScene()
	GameConfiguration.LoadedLevel.free()
	GameConfiguration.LoadedLevel = null
	
	if GameConfiguration._game_mode == GameConfiguration.GameMode.Multiplayer:
		emit_signal( "leave_lobby" )
	
	print( "Loading main menu..." )
	
	var scene:PackedScene = load( "res://scenes/main_menu.tscn" )
	var node:Node = scene.instantiate()
	get_tree().current_scene.queue_free()
	get_tree().root.add_child( node )
	get_tree().current_scene = node

func _on_confirm_exit_canceled() -> void:
	_confirm_exit_dlg.hide()
	_confirm_dlg_overlay.hide()

func _on_confirm_quit_confirmed() -> void:
	_confirm_dlg_overlay.hide()
	get_tree().quit()

func _on_confirm_quit_canceled() -> void:
	_confirm_quit_dlg.hide()
	_confirm_dlg_overlay.hide()
