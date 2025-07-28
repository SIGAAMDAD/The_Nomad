@tool
extends EditorPlugin

func _enter_tree():
	# Initialization of the plugin goes here.
	# Add the new type with a name, a parent type, a script and an icon.
	add_custom_type("SmartCamera2D", "Camera2D", preload("SmartCamera2D.gd"), preload("Camera2D.svg"))
	add_autoload_singleton("CameraControl", "CameraControl.gd")

func _exit_tree():
	# Clean-up of the plugin goes here.
	# Always remember to remove it from the engine when deactivated.
	remove_custom_type("SmartCamera2D")
	remove_autoload_singleton("CameraControl")
