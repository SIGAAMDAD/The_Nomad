class_name SettingsMenu extends Control

class SettingsConfig:
	#
	# video options
	#
	var _window_mode
	var _drs_preset
	var _aspect_ratio
	var _resolution

	#
	# graphics options
	#
	var _particle_quality
	var _animation_quality
	var _anti_aliasing
	
	func _init() -> void:
		pass

var _description_label: RichTextLabel

var _vsync: HBoxContainer
var _resolution_option: HBoxContainer
var _window_mode_option: HBoxContainer
var _anti_aliasing_option: HBoxContainer