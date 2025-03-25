class_name RebindButton extends Control

@onready var _name:Label = $HBoxContainer/RebindNameLabel
@onready var _label:RichTextLabel = $HBoxContainer/RebindButtonLabel
#@onready var _button = $HBoxContainer/RebindButton as Button
@onready var _input_detector:GUIDEInputDetector = %GUIDEInputDetector

@export var _action_label:StringName = ""
@export var _action_name:StringName = ""
@export var _display_category:StringName = ""
@export var _action:GUIDEAction = null

func _rebind( event: InputEvent, item: GUIDERemapper.ConfigItem ) -> void:
	if event is not InputEventMouseButton:
		return
	elif event.button_index != MOUSE_BUTTON_LEFT:
		return
	
	_input_detector.detect( item.value_type, \
		[ GUIDEInputDetector.DeviceType.KEYBOARD, GUIDEInputDetector.DeviceType.MOUSE, GUIDEInputDetector.DeviceType.JOY ] )
	
	_label.parse_bbcode( "Press Any Key..." )
	var input:GUIDEInput = await _input_detector.input_detected
	if input == null:
		return
	
	var collisions:Array[ GUIDERemapper.ConfigItem ] = SettingsData.GetRemapper().get_input_collisions( item, input )
	if collisions.any( func( it: GUIDERemapper.ConfigItem ): return not it.is_remappable ):
		return
	
	for collision in collisions:
		SettingsData.GetRemapper().set_bound_input( collision, null )
	
	SettingsData.GetRemapper().set_bound_input( item, input )
	_label.parse_bbcode( await SettingsData.GetMappingFormatter().input_as_richtext_async( input ) )

func _ready() -> void:
	var items:Array[ GUIDERemapper.ConfigItem ] = SettingsData.GetRemapper().get_remappable_items( null, "", _action )
	var input:GUIDEInput = SettingsData.GetRemapper().get_bound_input_or_null( items[0] )
	if input == null:
		_label.text = "Not bound"
	else:
		_label.parse_bbcode( await SettingsData.GetMappingFormatter().input_as_richtext_async( input ) )
	
	_name.text = _action_label
	
	var event:InputEvent
	_label.gui_input.connect( Callable( _rebind ).bind( items[0] ) )
