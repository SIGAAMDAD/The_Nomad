@tool
extends Node
signal success

enum TimeLimitResetMode {
	## In this mode, the TimeLimit will
	## start on the first successful inputs and 
	## not refresh on further successful inputs.
	##[br]Effectively, a timelimit for the entire combination.
	NO_REFRESH,
	## In this mode, the TimeLimit will
	## reset on every successful input.
	REFRESH_ON_SUCCESS,
}

## The default, the classic.
const KONAMI_CODE_KEYBOARD: Array[Key] = [
	KEY_UP, KEY_UP, KEY_DOWN, KEY_DOWN,
	KEY_LEFT, KEY_RIGHT, KEY_LEFT, KEY_RIGHT,
	KEY_A, KEY_B, KEY_ENTER 
]

const KONAMI_CODE_CONTROLLER: Array[JoyButton] = [
	JOY_BUTTON_DPAD_UP, JOY_BUTTON_DPAD_UP,
	JOY_BUTTON_DPAD_DOWN, JOY_BUTTON_DPAD_DOWN,
	JOY_BUTTON_DPAD_LEFT, JOY_BUTTON_DPAD_RIGHT,
	JOY_BUTTON_DPAD_LEFT, JOY_BUTTON_DPAD_RIGHT,
	JOY_BUTTON_A, JOY_BUTTON_B, JOY_BUTTON_START
]

## Will it do the thing?
@export var active: bool = true

## If enabled the listener will disable itself
## once successfully triggered.
@export var fire_only_once: bool = false

@export_category("Time Limit")
@export var time_limit_mode: bool = false:
	set(value):
		time_limit_mode = value
		property_list_changed.emit()

## Connect your TimeLimit timer here.
@export var timer: Timer
## The mode your TimeLimit will have. 
##[br]See the enum itself for the different modes.
@export var reset_mode: TimeLimitResetMode = TimeLimitResetMode.NO_REFRESH

@export_category("Code")
## Determines if the input will be from a keyboard,
## or a controller.
@export var controller_mode: bool = false:
	set(value):
		controller_mode = value
		property_list_changed.emit()
## What you want your code to be.
##[br]If left empty, it defaults to the Konami Code.
##[br]Used when not in `controller_mode`.
@export var code_override_keyboard: Array[Key]
## What you want your code to be.
##[br]If left empty, it defaults to the Konami Code.
##[br]Used when in `controller_mode`.
@export var code_override_controller: Array[JoyButton]

var _relevant_code_keyboard: Array[Key]
var _relevant_code_controller: Array[JoyButton]
var _successful_presses: int = 0

## Friendship ended with _get_property_list(),
## now _validate_property() is my new best friend.
func _validate_property(property: Dictionary) -> void:
	if property.name == "code_override_controller":
		if controller_mode:
			property.usage |= PROPERTY_USAGE_EDITOR
		else:
			property.usage &= ~PROPERTY_USAGE_EDITOR
	
	if property.name == "code_override_keyboard":
		if controller_mode:
			property.usage &= ~PROPERTY_USAGE_EDITOR
		else:
			property.usage |= PROPERTY_USAGE_EDITOR
	
	if property.name == "timer":
		if time_limit_mode:
			property.usage |= PROPERTY_USAGE_EDITOR
		else:
			property.usage &= ~PROPERTY_USAGE_EDITOR
	
	if property.name == "reset_mode":
		if time_limit_mode:
			property.usage |= PROPERTY_USAGE_EDITOR
		else:
			property.usage &= ~PROPERTY_USAGE_EDITOR

func _ready() -> void:
	if Engine.is_editor_hint():
		return
	
	if controller_mode:
		if len(code_override_controller) > 0:
			_relevant_code_controller = code_override_controller
		else:
			_relevant_code_controller = KONAMI_CODE_CONTROLLER
	else:
		if len(code_override_keyboard) > 0:
			_relevant_code_keyboard = code_override_keyboard
		else:
			_relevant_code_keyboard = KONAMI_CODE_KEYBOARD
	
	if time_limit_mode:
		assert(timer, "In order to use TimeLimitMode you must have a timer connected.")

		timer.autostart = false
		timer.one_shot = true
		timer.timeout.connect(_on_timelimit_timeout)

func _input(event: InputEvent) -> void:
	if !active:
		return

	if controller_mode:
		if event is not InputEventJoypadButton:
			return
		
		if !event.pressed:
			return

	else:
		if event is not InputEventKey:
			return

		if event.echo || !event.pressed:
			return
	
	_code_checker(event)

func _code_checker(event: InputEvent) -> void:
	var actual_code: Array[int] = _relevant_code_controller if controller_mode \
	else _relevant_code_keyboard

	var thing_to_check: int = event.button_index if controller_mode else event.keycode

	if actual_code[_successful_presses] == thing_to_check:
		_successful_presses += 1
		_handle_timelimit()
	else:
		_successful_presses = 0
	
	if len(actual_code) == _successful_presses:
		success.emit()
		_successful_presses = 0

		if fire_only_once:
			disable()

func _handle_timelimit() -> void:
	if !time_limit_mode:
		return
	
	if reset_mode == TimeLimitResetMode.NO_REFRESH \
	&& _successful_presses > 1:
		return
	
	timer.start()

func _on_timelimit_timeout() -> void:
	_successful_presses = 0

## Below are functions you can use to change values.
## In case you're not the kinda person who wants to 
## directly go on the properties.

func enable() -> void:
	active = true

func disable() -> void:
	active = false
