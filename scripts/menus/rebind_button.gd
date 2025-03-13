class_name RebindButton extends Control

@onready var _label:RichTextLabel = $RebindLabel
#@onready var _button = $HBoxContainer/RebindButton as Button
@onready var _input_detector:GUIDEInputDetector = %GUIDEInputDetector

@export var _action_name:String = "move_left_0"
@export var _display_category:String = ""
@export var _action:GUIDEAction = null

const ACTION_TO_LABEL:Dictionary = {
	"move_left_0": "Move Left",
	"move_right_0": "Move Right",
	"move_up_0": "Move Up",
	"move_down_0": "Move Down",
	"dash_0": "Dash"
};

const KEY_ICONS:Dictionary = {
	"A": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_a.png",
	"B": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_b.png",
	"C": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_c.png",
	"D": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_d.png",
	"E": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_e.png",
	"F": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f.png",
	"G": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_g.png",
	"H": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_h.png",
	"I": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_i.png",
	"J": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_j.png",
	"K": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_k.png",
	"L": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_l.png",
	"M": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_m.png",
	"N": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_n.png",
	"O": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_o.png",
	"P": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_p.png",
	"Q": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_q.png",
	"R": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_r.png",
	"S": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_s.png",
	"T": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_t.png",
	"U": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_u.png",
	"V": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_v.png",
	"W": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_w.png",
	"X": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_x.png",
	"Y": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_y.png",
	"Z": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_z.png",
	"0": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_0.png",
	"1": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_1.png",
	"2": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_2.png",
	"3": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_3.png",
	"4": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_4.png",
	"5": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_5.png",
	"6": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_6.png",
	"7": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_7.png",
	"8": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_8.png",
	"9": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_9.png",
	"F1": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f1.png",
	"F2": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f2.png",
	"F3": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f3.png",
	"F4": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f4.png",
	"F5": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f5.png",
	"F6": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f6.png",
	"F7": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f7.png",
	"F8": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f8.png",
	"F9": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f9.png",
	"F10": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f10.png",
	"F11": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f11.png",
	"F12": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_f12.png",
	"Shift": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_shift.png",
	"Space": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_space.png",
	"Ctrl": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_ctrl.png",
	"Backspace": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_backspace.png",
	"Tab": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_tab.png",
	"Enter": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_enter.png",
	"Up": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_arrow_up.png",
	"Down": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_arrow_down.png",
	"Left": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_arrow_left.png",
	"Right": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/keyboard_arrow_right.png",
	"Left Button": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/mouse_left.png",
	"Right Button": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/mouse_right.png",
	"Wheel Down": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/mouse_wheel_down.png",
	"Wheel Up": "res://textures/kenney_input-prompts/Keyboard & Mouse/Default/mouse_wheel_up.png",
	"Joypad A": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_button_color_a.png",
	"Joypad B": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_button_color_b.png",
	"Joypad X": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_button_color_x.png",
	"Joypad Y": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_button_color_y.png",
	"Joypad Back": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_button_view.png",
	"Joypad Start": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_button_start.png",
	"Joypad LStick": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_l_press.png",
	"Joypad RStick": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_r_press.png",
	"Joypad Left Left": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_l_left.png",
	"Joypad Left Right": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_l_right.png",
	"Joypad Left Up": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_l_up.png",
	"Joypad Left Down": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_l_down.png",
	
	"Joypad Right Left": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_r_left.png",
	"Joypad Right Right": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_r_right.png",
	"Joypad Right Up": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_r_up.png",
	"Joypad Right Down": "res://textures/kenney_input-prompts/Xbox Series/Default/xbox_stick_r_down.png",
};

const JOYPAD_STRINGS:Dictionary = {
	JOY_BUTTON_A: "Joypad A",
	JOY_BUTTON_B: "Joypad B",
	JOY_BUTTON_X: "Joypad X",
	JOY_BUTTON_Y: "Joypad Y",
	JOY_BUTTON_BACK: "Joypad Back",
	JOY_BUTTON_START: "Joypad Start",
	JOY_BUTTON_LEFT_STICK: "Joypad LStick",
	JOY_BUTTON_RIGHT_STICK: "Joypad RStick"
};

func set_action_name() -> void:
	_label.text = ACTION_TO_LABEL[ _action_name ]

func set_text_for_bind() -> void:
	var actionEvents = InputMap.action_get_events( _action_name )
	var actionEvent = actionEvents[0]
	var text:String
	
	if actionEvent is InputEventKey:
		var actionKeyCode = OS.get_keycode_string( actionEvent.physical_keycode )
		text = "%s" % actionKeyCode
	elif actionEvent is InputEventMouseButton:
		if actionEvent.button_index == MOUSE_BUTTON_LEFT:
			text = "Left Button"
		elif actionEvent.button_index == MOUSE_BUTTON_RIGHT:
			text = "Right Button"
		elif actionEvent.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			text = "Wheel Down"
		elif actionEvent.button_index == MOUSE_BUTTON_WHEEL_UP:
			text = "Wheel Up"
#	elif actionEvent is InputEventJoypadButton:
#		text = JOYPAD_STRINGS[ actionEvent.button_index ]
#	elif actionEvent is InputEventJoypadMotion:
#		if actionEvent.axis_value < 0.0:
#			match actionEvent.axis:
#				JOY_AXIS_LEFT_X:
#					text = "Joypad Left Left"
#				JOY_AXIS_RIGHT_X:
#					text = "Joypad Right Left"
#				JOY_AXIS_LEFT_Y:
#					text = "Joypad Left Up"
#				JOY_AXIS_RIGHT_Y:
#					text = "Joypad Right Up"
#		elif actionEvent.axis_value > 0.0:
#			match actionEvent.axis:
#				JOY_AXIS_LEFT_X:
#					text = "Joypad Left Right"
#				JOY_AXIS_RIGHT_X:
#					text = "Joypad Right Right"
#				JOY_AXIS_LEFT_Y:
#					text = "Joypad Left Down"
#				JOY_AXIS_RIGHT_Y:
#					text = "Joypad Right Down"
	
#	_button.icon = ResourceLoader.load( KEY_ICONS[ text ] )

func _rebind( event: InputEvent, item: GUIDERemapper.ConfigItem ) -> void:
	if event is not InputEventMouseButton:
		return
	elif event.button_index != MOUSE_BUTTON_LEFT:
		return
	
	_input_detector.detect( item.value_type )
	
	_label.text = "Press Any Key..."
	var input:GUIDEInput = await _input_detector.input_detected
	if input == null:
		return
	
	var collisions:Array[ GUIDERemapper.ConfigItem ] = SettingsData._remapper.get_input_collisions( item, input )
	if collisions.any( func( it: GUIDERemapper.ConfigItem ): return not it.is_remappable ):
		return
	
	for collision in collisions:
		SettingsData._remapper.set_bound_input( collision, null )
	
	SettingsData._remapper.set_bound_input( item, input )
	_label.parse_bbcode( await SettingsData._mapping_formatter.input_as_richtext_async( input ) )

func _ready() -> void:
	var items:Array[ GUIDERemapper.ConfigItem ] = SettingsData._remapper.get_remappable_items( null, "", _action )
	var input:GUIDEInput = SettingsData._remapper.get_bound_input_or_null( items[0] )
	if input == null:
		_label.text = "Not bound"
	else:
		_label.parse_bbcode( await SettingsData._mapping_formatter.input_as_richtext_async( input ) )
	
	var tmp:GUIDERemapper.ConfigItem
	var event:InputEvent
	_label.gui_input.connect( Callable( _rebind ).bind( event, tmp ) )
