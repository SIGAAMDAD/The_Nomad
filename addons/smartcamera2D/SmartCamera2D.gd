@tool
extends Camera2D

class_name SmartCamera2D

enum TARGET_MODE {
	PARENT,
	SINGLE,
	MULTIPLE,
	GROUP
}

@export_category("GENERAL CONFIG")
@export var default_zoom: Vector2 = Vector2.ONE

@export_category("TARGET CONFIG")
@export var target_mode: TARGET_MODE:
	set(value):
		target_mode = value
		notify_property_list_changed()

@export var target: NodePath
@export var targets: Array[NodePath]
@export var group_name: String
@export var adjust_zoom: bool = true
@export_range(1.0, 999.9) var adjust_zoom_margin: float
@export_range(0.001, 999.9) var adjust_zoom_min: float = 0.1
@export_range(0.001, 999.9) var adjust_zoom_max: float = 1.0
@export_range(-1, 999, 1) var adjust_zoom_priority_node_index: int = -1

@export_category("EFFECTS CONFIG")
@export var effects_layer: int = 2

var target_node: Node2D
var target_nodes: Array[Node]
var extra_position = Vector2.ZERO

@onready var refresh_target_timer = Timer.new()
@onready var canvas_layer = CanvasLayer.new()
@onready var camera_flash: ColorRect

func _ready():
	if Engine.is_editor_hint():
		return
	CameraControl.apply_flash.connect(apply_camera_flash)
	CameraControl.apply_shake.connect(apply_camera_shake)
	zoom = default_zoom
	_make_refresh_timer()
	_make_effects_layers()
	if target_mode == TARGET_MODE.SINGLE:
		ensure_target_node()
	elif target_mode == TARGET_MODE.MULTIPLE:
		ensure_target_nodes()
	call_deferred("_refresh_targets")

func _make_refresh_timer():
	refresh_target_timer.connect("timeout", _refresh_targets)
	refresh_target_timer.wait_time = 0.3
	add_child(refresh_target_timer)
	refresh_target_timer.start()

func _make_effects_layers():
	canvas_layer.layer = effects_layer
	add_child(canvas_layer)
	camera_flash = create_color_rect()

func apply_camera_shake(force: float = 2.0, duration = 0.8):
	var tween = create_tween()
	var step_dur = duration / 5.0
	tween.tween_property(self, "extra_position", Vector2(-force, force), step_dur)
	tween.tween_property(self, "extra_position", Vector2(force, force), step_dur)
	tween.tween_property(self, "extra_position", Vector2(force, -force), step_dur)
	tween.tween_property(self, "extra_position", Vector2(-force, -force), step_dur)
	tween.tween_property(self, "extra_position", Vector2.ZERO, step_dur)

func apply_camera_flash(color: Color, duration = 0.3):
	var screen_size = get_viewport_rect().size
	camera_flash.size = screen_size
	var tween = create_tween()
	var final_color = Color(color, 0.0)
	var initial_color = Color(color, 0.2)
	camera_flash.visible = true
	tween.tween_property(camera_flash, "color", final_color, duration) \
		.from(initial_color)
	await tween.finished
	camera_flash.visible = false

func _process(delta):
	if not Engine.is_editor_hint():
		_sanitize_targets()
		if target_mode == TARGET_MODE.PARENT:
			position = Vector2.ZERO
		elif target_mode == TARGET_MODE.SINGLE:
			_process_target_single(delta)
		elif target_mode == TARGET_MODE.MULTIPLE:
			_process_target_multiple(delta)
		elif target_mode == TARGET_MODE.GROUP:
			_process_target_group(delta)
		position = position + extra_position
	
func _process_target_single(delta):
	if not target_node:
		printerr("[SmartCamera2D] TARGET NODE IS NULL")
		return
	global_position = target_node.global_position
	
func _process_target_multiple(delta):
	if target_nodes.size() < 0:
		printerr("[SmartCamera2D] NOT ENOUGH TARGET NODES")
		return
	adjust_camera_zoom()
	
func _process_target_group(delta):
	if target_nodes.size() < 0:
		printerr("[SmartCamera2D] NOT ENOUGH TARGET NODES")
		return
	adjust_camera_zoom()

func ensure_target_node():
	if target_node: return
	target_node = get_node(target)

func ensure_target_nodes():
	target_nodes.clear()
	for path in targets:
		target_nodes.append(get_node(path))

func adjust_camera_zoom():
	if target_nodes.is_empty():
		return
	if target_nodes.size() == 1:
		zoom = default_zoom
		global_position = target_nodes[0].global_position
		return
	var screen_size = get_viewport_rect().size
	var min_pos = target_nodes[0].global_position
	var max_pos = target_nodes[0].global_position
	for node in target_nodes:
		min_pos = min_pos.min(node.global_position)
		max_pos = max_pos.max(node.global_position)
	var size = max_pos - min_pos
	var zoom_x = screen_size.x / size.x if size.x != 0 else 1.0
	var zoom_y = screen_size.y / size.y if size.y != 0 else 1.0
	var zoom_safe = clamp(min(zoom_x, zoom_y) / adjust_zoom_margin, adjust_zoom_min, adjust_zoom_max)
	zoom = Vector2(zoom_safe, zoom_safe)
	var center_position = (min_pos + max_pos) / 2.0
	global_position = center_position
	
	var priority_i = adjust_zoom_priority_node_index
	if priority_i > -1:
		if not is_point_visible(target_nodes[priority_i].global_position, screen_size):
			center_position = target_nodes[priority_i].global_position

func is_point_visible(point: Vector2, screen_size: Vector2) -> bool:
	var half_screen = (screen_size / 2.0) / zoom
	var camera_min = global_position - half_screen
	var camera_max = global_position + half_screen
	return point.x >= camera_min.x and point.x <= camera_max.x and point.y >= camera_min.y and point.y <= camera_max.y

func create_color_rect():
	var color_rect = ColorRect.new()
	color_rect.visible = false
	color_rect.color = Color.TRANSPARENT
	color_rect.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	color_rect.size_flags_vertical = Control.SIZE_EXPAND_FILL
	color_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas_layer.add_child(color_rect)
	return color_rect

func _refresh_targets():
	if target_mode == TARGET_MODE.SINGLE:
		if not target_node:
			target_node = get_node(target)
	elif target_mode == TARGET_MODE.GROUP:
		target_nodes = get_tree().get_nodes_in_group(group_name)

func _sanitize_targets():
	if target_node and target_node.is_queued_for_deletion():
		target_node = null
	if not target_nodes.is_empty():
		target_nodes = target_nodes.filter(_filter_existing_node)

func _filter_existing_node(variant) -> bool:
	if variant == null:
		return false
	if not variant is Node2D:
		return false
	if variant is Node2D and variant.is_queued_for_deletion():
		return false
	return true


func _validate_property(property: Dictionary):
	var multiple_properties = [
		"adjust_zoom", "zoom_margin", "adjust_zoom_margin",
		"adjust_zoom_min", "adjust_zoom_max", "adjust_zoom_priority_node_index"
	]
	if property.name == "target" and target_mode != TARGET_MODE.SINGLE:
		property.usage = PROPERTY_USAGE_NO_EDITOR
	if property.name == "group_name" and target_mode != TARGET_MODE.GROUP:
		property.usage = PROPERTY_USAGE_NO_EDITOR
	if property.name == "targets" and target_mode != TARGET_MODE.MULTIPLE:
		property.usage = PROPERTY_USAGE_NO_EDITOR
	if property.name in multiple_properties and target_mode not in [TARGET_MODE.MULTIPLE, TARGET_MODE.GROUP]:
		property.usage = PROPERTY_USAGE_NO_EDITOR
