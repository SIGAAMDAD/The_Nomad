extends Node2D
class_name HeightPassRenderer2D

@export var source_root: Node
@export var main_camera: Camera2D

@export var height_max_px: float = 256.0
@export var default_height_scale: float = 1.0
@export var default_contact_band_uv: float = 0.10

# If true, every Sprite2D / AnimatedSprite2D under source_root can cast.
# If false, only nodes in caster_group are drawn.
@export var auto_include_all_sprites := false
@export var caster_group := "height_shadow_caster"

@export var cull_margin_px: float = 256.0

var _casters: Array[CanvasItem] = []

func _ready() -> void:
	process_priority = 100000
	_rebuild_caster_cache()

func _process(_delta: float) -> void:
	queue_redraw()

func _rebuild_caster_cache() -> void:
	_casters.clear()

	if source_root == null:
		return

	if auto_include_all_sprites:
		_collect_sprites(source_root)
	else:
		for node in get_tree().get_nodes_in_group(caster_group):
			if node is Sprite2D or node is AnimatedSprite2D:
				_casters.append(node)

func _collect_sprites(node: Node) -> void:
	if node is Sprite2D or node is AnimatedSprite2D:
		if not bool(node.get_meta("height_shadow_disabled", false)):
			_casters.append(node)

	for child in node.get_children():
		_collect_sprites(child)

func _draw() -> void:
	if source_root == null:
		return

	var self_inv := global_transform.affine_inverse()

	for caster in _casters:
		if not is_instance_valid(caster):
			continue

		if not caster.is_visible_in_tree():
			continue

		var draw_data := _get_draw_data(caster)

		if draw_data.is_empty():
			continue

		var texture: Texture2D = draw_data["texture"]
		var local_rect: Rect2 = draw_data["local_rect"]
		var source_rect: Rect2 = draw_data["source_rect"]
		var sprite_height_px: float = draw_data["sprite_height_px"]

		if texture == null:
			continue

		if not _is_visible_on_screen(caster, local_rect):
			continue

		var height_scale := float(caster.get_meta("height_scale", default_height_scale))
		var contact_band_uv := float(caster.get_meta("contact_band_uv", default_contact_band_uv))

		var screen_height_px := _estimate_screen_height_px(caster, sprite_height_px)
		var normalized_height: float = clamp((screen_height_px * height_scale) / height_max_px, 0.0, 1.0)

		var source_alpha := _get_canvas_item_alpha(caster)

		# Pack per-caster params into vertex color:
		# R = normalized height
		# G = contact band
		# A = alpha
		var packed_params := Color(
			normalized_height,
			clamp(contact_band_uv, 0.001, 1.0),
			0.0,
			source_alpha
		)

		draw_set_transform_matrix(self_inv * caster.global_transform)
		draw_texture_rect_region(texture, local_rect, source_rect, packed_params)

	draw_set_transform_matrix(Transform2D.IDENTITY)

func _get_draw_data(caster: CanvasItem) -> Dictionary:
	if caster is Sprite2D:
		return _get_sprite2d_draw_data(caster as Sprite2D)

	if caster is AnimatedSprite2D:
		return _get_animated_sprite2d_draw_data(caster as AnimatedSprite2D)

	return {}

func _get_sprite2d_draw_data(sprite: Sprite2D) -> Dictionary:
	var texture := sprite.texture

	if texture == null:
		return {}

	var texture_size := texture.get_size()
	var source_rect := Rect2(Vector2.ZERO, texture_size)
	var sprite_size := texture_size

	if sprite.region_enabled:
		source_rect = sprite.region_rect
		sprite_size = sprite.region_rect.size
	elif sprite.hframes > 1 or sprite.vframes > 1:
		var frame_size := Vector2(
			texture_size.x / float(sprite.hframes),
			texture_size.y / float(sprite.vframes)
		)

		var frame_coords := sprite.frame_coords
		source_rect = Rect2(
			Vector2(frame_coords.x * frame_size.x, frame_coords.y * frame_size.y),
			frame_size
		)

		sprite_size = frame_size

	var local_pos := sprite.offset

	if sprite.centered:
		local_pos -= sprite_size * 0.5

	var local_rect := Rect2(local_pos, sprite_size)

	# Basic flip handling.
	if sprite.flip_h:
		local_rect.position.x += local_rect.size.x
		local_rect.size.x *= -1.0

	if sprite.flip_v:
		local_rect.position.y += local_rect.size.y
		local_rect.size.y *= -1.0

	return {
		"texture": texture,
		"local_rect": local_rect,
		"source_rect": source_rect,
		"sprite_height_px": abs(sprite_size.y)
	}

func _get_animated_sprite2d_draw_data(sprite: AnimatedSprite2D) -> Dictionary:
	var frames := sprite.sprite_frames

	if frames == null:
		return {}

	var texture := frames.get_frame_texture(sprite.animation, sprite.frame)

	if texture == null:
		return {}

	var sprite_size := texture.get_size()
	var source_rect := Rect2(Vector2.ZERO, sprite_size)

	var local_pos := sprite.offset

	if sprite.centered:
		local_pos -= sprite_size * 0.5

	var local_rect := Rect2(local_pos, sprite_size)

	if sprite.flip_h:
		local_rect.position.x += local_rect.size.x
		local_rect.size.x *= -1.0

	if sprite.flip_v:
		local_rect.position.y += local_rect.size.y
		local_rect.size.y *= -1.0

	return {
		"texture": texture,
		"local_rect": local_rect,
		"source_rect": source_rect,
		"sprite_height_px": abs(sprite_size.y)
	}

func _estimate_screen_height_px(caster: CanvasItem, local_height_px: float) -> float:
	# Converts source local sprite height into current viewport pixel height.
	# This automatically follows camera zoom, object scale, parent transforms, etc.
	var xform := caster.get_global_transform_with_canvas()

	var p0 := xform * Vector2.ZERO
	var p1 := xform * Vector2(0.0, local_height_px)

	return p0.distance_to(p1)

func _get_canvas_item_alpha(caster: CanvasItem) -> float:
	var alpha := caster.modulate.a * caster.self_modulate.a

	var parent := caster.get_parent()

	while parent != null:
		if parent is CanvasItem:
			alpha *= (parent as CanvasItem).modulate.a
		parent = parent.get_parent()

	return clamp(alpha, 0.0, 1.0)

func _is_visible_on_screen(caster: CanvasItem, local_rect: Rect2) -> bool:
	var xform := caster.get_global_transform_with_canvas()

	var points := [
		xform * local_rect.position,
		xform * (local_rect.position + Vector2(local_rect.size.x, 0.0)),
		xform * (local_rect.position + Vector2(0.0, local_rect.size.y)),
		xform * (local_rect.position + local_rect.size)
	]

	var min_p: Vector2 = points[0]
	var max_p: Vector2 = points[0]

	for p in points:
		min_p = min_p.min(p)
		max_p = max_p.max(p)

	var screen_rect := Rect2(min_p, max_p - min_p).grow(cull_margin_px)
	var viewport_rect := get_viewport_rect()

	return screen_rect.intersects(viewport_rect)
