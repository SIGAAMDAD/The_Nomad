class_name KeybindResource extends Resource

const MOVE_LEFT:String = "move_left_0"
const MOVE_RIGHT:String = "move_right_0"
const MOVE_UP:String = "move_up_0"
const MOVE_DOWN:String = "move_down_0"
const DASH:String = "dash_0"

@export var _DEFAULT_MOVE_LEFT_KEY:InputEventKey = InputEventKey.new()
@export var _DEFAULT_MOVE_RIGHT_KEY:InputEvent = InputEventKey.new()
@export var _DEFAULT_MOVE_UP_KEY:InputEvent = InputEventKey.new()
@export var _DEFAULT_DASH_KEY:InputEvent = InputEventKey.new()
@export var _DEFAULT_MOVE_DOWN_KEY:InputEvent = InputEventKey.new()
@export var _DEFAULT_JUMP_KEY:InputEvent = InputEventKey.new()
@export var _DEFAULT_USE_WEAPON_KEY:InputEvent = InputEventMouseButton.new()

var _move_left_key:InputEvent = InputEventKey.new()
var _move_right_key:InputEvent = InputEventKey.new()
var _move_up_key:InputEvent = InputEventKey.new()
var _move_down_key:InputEvent = InputEventKey.new()
var _dash_key:InputEvent = InputEventKey.new()
var _use_weapon_key:InputEvent = InputEventMouseButton.new()
