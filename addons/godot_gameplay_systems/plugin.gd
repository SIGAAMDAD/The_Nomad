@tool
extends EditorPlugin

const attributes_and_abilities_plugin_script = preload("./attributes_and_abilities/plugin.gd")
const extended_character_nodes_script = preload("./extended_character_nodes/plugin.gd")
const inventory_system_script = preload("./inventory_system/plugin.gd")
const interactables_script = preload("./interactables/plugin.gd")
const slideshow_script = preload("./slideshow/plugin.gd")


var attributes_and_abilities_plugin: EditorPlugin
var extended_character_nodes: EditorPlugin
var interactables: EditorPlugin
var slideshow: EditorPlugin


func _init() -> void:
	attributes_and_abilities_plugin = attributes_and_abilities_plugin_script.new()
	extended_character_nodes = extended_character_nodes_script.new()
	interactables = interactables_script.new()
	slideshow = slideshow_script.new()


func _enter_tree():
	attributes_and_abilities_plugin._enter_tree()
	extended_character_nodes._enter_tree()
	interactables._enter_tree()
	slideshow._enter_tree()

func _exit_tree():
	attributes_and_abilities_plugin._exit_tree()
	extended_character_nodes._exit_tree()
	interactables._exit_tree()
	slideshow._exit_tree()
