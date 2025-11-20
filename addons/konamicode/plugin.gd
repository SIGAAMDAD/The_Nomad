@tool
extends EditorPlugin


func _enter_tree():
	# Initialization of the plugin goes here.
	add_custom_type("KonamiCode", "Node", preload("konamicode.gd"), preload("icon.svg"))

func _exit_tree():
	remove_custom_type("KonamiCode")