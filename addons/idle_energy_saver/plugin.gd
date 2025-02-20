@tool
extends EditorPlugin


func _enter_tree() -> void:
	add_custom_type("IdleEnergySaver", "Node", load("res://addons/idle_energy_saver/idle_energy_saver.gd"), load("res://addons/idle_energy_saver/bottled-bolt.svg"))


func _exit_tree() -> void:
	remove_custom_type("IdleEnergySaver")
