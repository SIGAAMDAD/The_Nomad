extends Node3D
class_name World2DTo3DPresenter

## Mirrors the existing authoritative 2D world into a flat 3D presentation layer.
##
## The gameplay tree remains 2D: physics, spawning, interaction, and Camera2D code keep
## running as before. This node creates Sprite3D/MeshInstance3D presentation copies so
## DirectionalLight3D, WorldEnvironment, glow, fog, and 3D shadows can affect the scene.

const ORIGINAL_SELF_MODULATE_META := "__single_world_2_5d_original_self_modulate"
const DISABLE_PRESENTATION_META := "disable_2_5d_presentation"

@export_group("Source")
@export var source_root: Node
@export var hide_source_canvas_items := true
@export var rescan_interval_frames := 15

@export_group("3D Scene")
@export var camera_3d: Camera3D
@export var sun_2d: DirectionalLight2D
@export var sun_3d: DirectionalLight3D
@export var ambient_2d: CanvasModulate
@export var world_environment: WorldEnvironment

@export_group("Projection")
@export_range(1.0, 512.0, 1.0) var pixels_per_meter := 100.0
@export var camera_distance := 96.0
@export var y_depth_step := 0.00012
@export var z_index_depth_step := 0.075
@export var background_depth_offset := 0.35

@export_group("Sprite Rendering")
@export var mirror_sprites := true
@export var mirror_polygons := true
@export var shaded_sprites := true
@export var sprite_alpha_cut_mode := SpriteBase3D.ALPHA_CUT_OPAQUE_PREPASS
@export_range(0.0, 1.0, 0.01) var alpha_scissor_threshold := 0.08
@export var use_nearest_texture_filter := true

@export_group("Lighting")
@export_range(0.0, 1.0, 0.01) var minimum_sun_height := 0.08
@export_range(0.0, 1.0, 0.01) var ambient_energy := 0.62
@export var sync_environment_from_canvas_modulate := true

var _pixel_size := 0.01
var _frame_count := 0
var _mirrors: Dictionary = {}
var _masked_items: Array[CanvasItem] = []

func _ready() -> void:
	process_priority = 200000
	_pixel_size = 1.0 / max(pixels_per_meter, 1.0)
	_configure_3d_defaults()
	_rebuild_mirror_cache()

func _exit_tree() -> void:
	_restore_masked_source_items()

func _process(_delta: float) -> void:
	_frame_count += 1

	if rescan_interval_frames > 0 and _frame_count % rescan_interval_frames == 0:
		_rebuild_mirror_cache()

	_sync_camera()
	_sync_sunlight()
	_sync_environment()
	_sync_mirrors()

func _configure_3d_defaults() -> void:
	if camera_3d != null:
		camera_3d.projection = Camera3D.PROJECTION_ORTHOGONAL
		camera_3d.near = 0.01
		camera_3d.far = max(camera_distance * 4.0, 512.0)
		camera_3d.current = true

	if sun_3d != null:
		sun_3d.shadow_enabled = true
		sun_3d.directional_shadow_mode = DirectionalLight3D.SHADOW_ORTHOGONAL
		sun_3d.directional_shadow_max_distance = max(camera_distance * 2.0, 128.0)
		sun_3d.directional_shadow_pancake_size = max(camera_distance, 32.0)

	if world_environment != null and world_environment.environment != null:
		var env := world_environment.environment
		env.set("glow_enabled", true)
		env.set("adjustment_enabled", true)

func _rebuild_mirror_cache() -> void:
	if source_root == null:
		return

	var found: Array[CanvasItem] = []
	_collect_visual_items(source_root, found)

	for item in found:
		if not _mirrors.has(item):
			_create_mirror_for(item)

	for item in _mirrors.keys():
		if not is_instance_valid(item) or not found.has(item):
			_destroy_mirror_for(item)

func _collect_visual_items(node: Node, out_items: Array[CanvasItem]) -> void:
	if node == null:
		return

	if node is CanvasItem and not bool(node.get_meta(DISABLE_PRESENTATION_META, false)):
		var canvas_item := node as CanvasItem

		if mirror_sprites and (canvas_item is Sprite2D or canvas_item is AnimatedSprite2D):
			out_items.append(canvas_item)
		elif mirror_polygons and canvas_item is Polygon2D:
			out_items.append(canvas_item)

	for child in node.get_children():
		_collect_visual_items(child, out_items)

func _create_mirror_for(item: CanvasItem) -> void:
	if item is Sprite2D or item is AnimatedSprite2D:
		var sprite := Sprite3D.new()
		sprite.name = "%s_3D" % item.name
		sprite.pixel_size = _pixel_size
		sprite.shaded = shaded_sprites
		sprite.double_sided = true
		sprite.transparent = true
		sprite.no_depth_test = false
		sprite.alpha_cut = sprite_alpha_cut_mode
		sprite.alpha_scissor_threshold = alpha_scissor_threshold
		sprite.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_ON

		if use_nearest_texture_filter:
			sprite.texture_filter = BaseMaterial3D.TEXTURE_FILTER_NEAREST

		add_child(sprite)
		_mirrors[item] = {
			"kind": "sprite",
			"node": sprite
		}
	elif item is Polygon2D:
		var mesh_instance := MeshInstance3D.new()
		mesh_instance.name = "%s_3D" % item.name
		mesh_instance.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF

		var material := StandardMaterial3D.new()
		material.cull_mode = BaseMaterial3D.CULL_DISABLED
		material.vertex_color_use_as_albedo = true
		material.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA

		if use_nearest_texture_filter:
			material.texture_filter = BaseMaterial3D.TEXTURE_FILTER_NEAREST

		mesh_instance.material_override = material
		add_child(mesh_instance)

		_mirrors[item] = {
			"kind": "polygon",
			"node": mesh_instance,
			"material": material
		}

	if hide_source_canvas_items:
		_mask_source_item(item)

func _destroy_mirror_for(item: CanvasItem) -> void:
	if _mirrors.has(item):
		var record: Dictionary = _mirrors[item]
		var node := record.get("node") as Node
		if is_instance_valid(node):
			node.queue_free()
		_mirrors.erase(item)

func _sync_mirrors() -> void:
	for item in _mirrors.keys():
		if not is_instance_valid(item):
			_destroy_mirror_for(item)
			continue

		var record: Dictionary = _mirrors[item]
		var kind := String(record.get("kind", ""))

		if kind == "sprite":
			_sync_sprite(item, record.get("node") as Sprite3D)
		elif kind == "polygon":
			_sync_polygon(item as Polygon2D, record)

func _sync_sprite(source: CanvasItem, mirror: Sprite3D) -> void:
	if mirror == null:
		return

	mirror.visible = source.is_visible_in_tree() and _get_unmasked_effective_modulate(source).a > 0.001

	if source is Sprite2D:
		var sprite_2d := source as Sprite2D
		mirror.texture = sprite_2d.texture
		mirror.hframes = sprite_2d.hframes
		mirror.vframes = sprite_2d.vframes
		mirror.frame = sprite_2d.frame
		mirror.region_enabled = sprite_2d.region_enabled
		mirror.region_rect = sprite_2d.region_rect
		mirror.centered = sprite_2d.centered
		mirror.offset = sprite_2d.offset
		mirror.flip_h = sprite_2d.flip_h
		mirror.flip_v = sprite_2d.flip_v
	elif source is AnimatedSprite2D:
		var animated_2d := source as AnimatedSprite2D
		var frames := animated_2d.sprite_frames
		if frames != null and frames.has_animation(animated_2d.animation) and frames.get_frame_count(animated_2d.animation) > 0:
			mirror.texture = frames.get_frame_texture(animated_2d.animation, animated_2d.frame)
		else:
			mirror.texture = null

		mirror.hframes = 1
		mirror.vframes = 1
		mirror.frame = 0
		mirror.region_enabled = false
		mirror.centered = animated_2d.centered
		mirror.offset = animated_2d.offset
		mirror.flip_h = animated_2d.flip_h
		mirror.flip_v = animated_2d.flip_v

	mirror.modulate = _get_unmasked_effective_modulate(source)
	var source_xform := source.get_global_transform()
	mirror.global_position = _canvas_position_to_3d(source_xform.origin, _depth_for_item(source))
	mirror.rotation = Vector3(0.0, 0.0, -source_xform.get_rotation())

	var scale_2d := source_xform.get_scale()
	mirror.scale = Vector3(scale_2d.x, scale_2d.y, 1.0)
	mirror.render_priority = int(clamp(float(source.z_index), -128.0, 127.0))

func _sync_polygon(source: Polygon2D, record: Dictionary) -> void:
	var mesh_instance := record.get("node") as MeshInstance3D
	if mesh_instance == null:
		return

	mesh_instance.visible = source.is_visible_in_tree() and _get_unmasked_effective_modulate(source).a > 0.001
	mesh_instance.mesh = _build_polygon_mesh(source)

	var material := record.get("material") as StandardMaterial3D
	if material != null:
		material.albedo_texture = source.texture
		material.albedo_color = _get_unmasked_effective_modulate(source)

func _build_polygon_mesh(source: Polygon2D) -> ArrayMesh:
	var mesh := ArrayMesh.new()
	var polygon := source.polygon

	if polygon.size() < 3:
		return mesh

	var indices := Geometry2D.triangulate_polygon(polygon)
	if indices.size() < 3:
		return mesh

	var vertices := PackedVector3Array()
	var normals := PackedVector3Array()
	var uvs := PackedVector2Array()
	var colors := PackedColorArray()

	var texture_size := Vector2.ONE
	if source.texture != null:
		texture_size = source.texture.get_size().max(Vector2.ONE)

	var vertex_colors := source.vertex_colors
	var polygon_uv := source.uv
	var base_color := source.color * _get_unmasked_effective_modulate(source)
	var depth := _depth_for_item(source) - background_depth_offset

	for index in indices:
		var local_point: Vector2 = polygon[index]
		var world_point: Vector2 = source.get_global_transform() * local_point

		vertices.append(_canvas_position_to_3d(world_point, depth))
		normals.append(Vector3(0.0, 0.0, 1.0))

		if polygon_uv.size() == polygon.size():
			uvs.append(polygon_uv[index] / texture_size)
		else:
			uvs.append(local_point / texture_size)

		if vertex_colors.size() == polygon.size():
			colors.append(vertex_colors[index] * _get_unmasked_effective_modulate(source))
		else:
			colors.append(base_color)

	var arrays := []
	arrays.resize(Mesh.ARRAY_MAX)
	arrays[Mesh.ARRAY_VERTEX] = vertices
	arrays[Mesh.ARRAY_NORMAL] = normals
	arrays[Mesh.ARRAY_TEX_UV] = uvs
	arrays[Mesh.ARRAY_COLOR] = colors

	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
	return mesh

func _sync_camera() -> void:
	if camera_3d == null:
		return

	var camera_2d := get_viewport().get_camera_2d()
	if camera_2d == null:
		return

	var viewport_size := get_viewport().get_visible_rect().size
	var screen_center := camera_2d.get_screen_center_position()
	var zoom_y: float = max(camera_2d.zoom.y, 0.001)
	var camera_depth := camera_distance + (screen_center.y * y_depth_step)

	camera_3d.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera_3d.size = max(viewport_size.y * _pixel_size / zoom_y, 0.001)
	camera_3d.global_position = _canvas_position_to_3d(screen_center, camera_depth)
	camera_3d.global_rotation = Vector3(0.0, 0.0, -camera_2d.global_rotation)
	camera_3d.current = true

func _sync_sunlight() -> void:
	if sun_2d == null or sun_3d == null:
		return

	sun_3d.visible = sun_2d.enabled
	sun_3d.light_color = sun_2d.color
	sun_3d.light_energy = sun_2d.energy
	sun_3d.shadow_enabled = sun_2d.shadow_enabled

	var sun_height: float = clamp(sun_2d.height, minimum_sun_height, 1.0)
	var planar_strength: float = max(1.0 - sun_height, 0.05)
	var travel_2d := Vector2.DOWN.rotated(sun_2d.global_rotation)
	var travel_3d := Vector3(
		travel_2d.x * planar_strength,
		-travel_2d.y * planar_strength,
		-sun_height
	).normalized()

	if travel_3d.length_squared() > 0.0001:
		sun_3d.global_position = Vector3.ZERO
		sun_3d.look_at(sun_3d.global_position + travel_3d, Vector3.UP)

func _sync_environment() -> void:
	if not sync_environment_from_canvas_modulate:
		return

	if world_environment == null or world_environment.environment == null or ambient_2d == null:
		return

	var env := world_environment.environment
	var ambient := ambient_2d.color

	env.set("background_color", ambient)
	env.set("ambient_light_color", ambient)
	env.set("ambient_light_energy", ambient_energy)
	env.set("ambient_light_source", Environment.AMBIENT_SOURCE_COLOR)

func _canvas_position_to_3d(position: Vector2, depth: float) -> Vector3:
	return Vector3(position.x * _pixel_size, -position.y * _pixel_size, depth)

func _depth_for_item(item: CanvasItem) -> float:
	var origin := item.get_global_transform().origin
	return (origin.y * y_depth_step) + (float(item.z_index) * z_index_depth_step)

func _get_unmasked_effective_modulate(item: CanvasItem) -> Color:
	var color := item.modulate * _get_unmasked_self_modulate(item)
	var parent := item.get_parent()

	while parent != null:
		if parent is CanvasItem:
			var parent_item := parent as CanvasItem
			color *= parent_item.modulate * _get_unmasked_self_modulate(parent_item)
		parent = parent.get_parent()

	return color

func _get_unmasked_self_modulate(item: CanvasItem) -> Color:
	if item.has_meta(ORIGINAL_SELF_MODULATE_META):
		var stored_modulate: Color = item.get_meta(ORIGINAL_SELF_MODULATE_META)
		return stored_modulate

	return item.self_modulate

func _mask_source_item(item: CanvasItem) -> void:
	if not item.has_meta(ORIGINAL_SELF_MODULATE_META):
		item.set_meta(ORIGINAL_SELF_MODULATE_META, item.self_modulate)
		_masked_items.append(item)

	var hidden := item.self_modulate
	hidden.a = 0.0
	item.self_modulate = hidden

func _restore_masked_source_items() -> void:
	for item in _masked_items:
		if is_instance_valid(item) and item.has_meta(ORIGINAL_SELF_MODULATE_META):
			var stored_modulate: Color = item.get_meta(ORIGINAL_SELF_MODULATE_META)
			item.self_modulate = stored_modulate
			item.remove_meta(ORIGINAL_SELF_MODULATE_META)

	_masked_items.clear()
