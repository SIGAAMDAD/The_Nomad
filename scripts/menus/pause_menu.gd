extends Control

@onready var _level:Node2D = $"../Level"
@onready var _confirm_exit_dlg:ConfirmationDialog = $ConfirmExit
@onready var _confirm_quit_dlg:ConfirmationDialog = $ConfirmQuit

var _paused:bool = false

func _process(delta: float) -> void:
	if Input.is_action_just_pressed( "ui_exit" ):
		pause_menu()

func pause_menu() -> void:
	if _paused:
		hide()
		Engine.time_scale = 1
	else:
		show()
		Engine.time_scale = 0
	
	_paused = !_paused;

func _on_resume_button_pressed() -> void:
	pause_menu()

func _on_exit_to_main_menu_button_pressed() -> void:
	_confirm_exit_dlg.show()

func _on_exit_game_button_pressed() -> void:
	_confirm_quit_dlg.show()

func _on_confirm_exit_confirmed() -> void:
	_paused = false
	Engine.time_scale = 1
	GameConfiguration.LoadedLevel.UnloadScene()
	GameConfiguration.LoadedLevel.free()
	GameConfiguration.LoadedLevel = null
	get_tree().change_scene_to_file( "res://scenes/main_menu.tscn" )

func _on_confirm_exit_canceled() -> void:
	_confirm_exit_dlg.hide()

func _on_confirm_quit_confirmed() -> void:
	get_tree().quit()

func _on_confirm_quit_canceled() -> void:
	_confirm_quit_dlg.hide()
