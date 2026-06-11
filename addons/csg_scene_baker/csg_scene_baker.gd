@tool
extends EditorPlugin

const MENU_ITEM_NAME := "Bake CSG Scene..."
const LIGHTMAP_TEXEL_SIZE := 0.05

var _folder_dialog: EditorFileDialog
var _is_baking := false


func _enter_tree() -> void:
	add_tool_menu_item(MENU_ITEM_NAME, Callable(self, "_open_output_folder_dialog"))

	_folder_dialog = EditorFileDialog.new()
	_folder_dialog.title = "Choose output folder for baked CSG scene"
	_folder_dialog.access = EditorFileDialog.ACCESS_RESOURCES
	_folder_dialog.file_mode = EditorFileDialog.FILE_MODE_OPEN_DIR
	_folder_dialog.current_dir = "res://"
	_folder_dialog.dir_selected.connect(_on_output_folder_selected)

	add_child(_folder_dialog)


func _exit_tree() -> void:
	remove_tool_menu_item(MENU_ITEM_NAME)

	if is_instance_valid(_folder_dialog):
		_folder_dialog.dir_selected.disconnect(_on_output_folder_selected)
		_folder_dialog.queue_free()
		_folder_dialog = null


func _open_output_folder_dialog() -> void:
	if _is_baking:
		push_warning("CSG bake is already running.")
		return

	var current_dir := EditorInterface.get_current_directory()
	if current_dir.begins_with("res://"):
		_folder_dialog.current_dir = current_dir
	else:
		_folder_dialog.current_dir = "res://"

	_folder_dialog.popup_centered(Vector2i(900, 700))


func _on_output_folder_selected(output_folder: String) -> void:
	if _is_baking:
		return

	_is_baking = true
	await _bake_current_scene(output_folder)
	_is_baking = false


func _bake_current_scene(output_folder: String) -> void:
	if output_folder.is_empty() or not output_folder.begins_with("res://"):
		push_error("Output folder must be inside res://.")
		return

	var edited_root := EditorInterface.get_edited_scene_root()
	if edited_root == null:
		push_error("No currently edited scene root found.")
		return

	# CSG mesh/collision data is deferred by one rendered frame.
	await get_tree().process_frame

	var bake_targets: Array[CSGShape3D] = []
	_collect_csg_bake_targets(edited_root, bake_targets)

	if bake_targets.is_empty():
		push_warning("No bakeable CSG root shapes found in the current scene.")
		return

	var baked_scene_root := Node3D.new()
	baked_scene_root.name = "%s_BakedCSG" % _make_safe_node_name(str(edited_root.name))

	var baked_count := 0

	for csg: CSGShape3D in bake_targets:
		if not is_instance_valid(csg):
			continue

		var visual_mesh := csg.bake_static_mesh()

		if visual_mesh == null or visual_mesh.get_surface_count() == 0:
			push_warning("Skipped '%s': bake_static_mesh() returned no surfaces." % str(csg.get_path()))
			continue

		visual_mesh.resource_name = "%s_BakedMesh" % str(csg.name)

		var unwrap_error := visual_mesh.lightmap_unwrap(csg.global_transform, LIGHTMAP_TEXEL_SIZE)
		if unwrap_error != OK:
			push_warning(
				"lightmap_unwrap() failed on '%s' with error: %s"
				% [str(csg.get_path()), str(unwrap_error)]
			)

		var collision_shape_resource := csg.bake_collision_shape()
		if collision_shape_resource == null:
			push_warning(
				"Skipped collision for '%s': bake_collision_shape() returned null."
				% str(csg.get_path())
			)
		else:
			collision_shape_resource.resource_name = "%s_BakedConcaveCollision" % str(csg.name)

		var static_body := StaticBody3D.new()
		static_body.name = "%s_StaticBody3D" % _make_safe_node_name(str(csg.name))
		static_body.global_transform = csg.global_transform
		static_body.collision_layer = csg.collision_layer
		static_body.collision_mask = csg.collision_mask
		static_body.collision_priority = csg.collision_priority

		var mesh_instance := MeshInstance3D.new()
		mesh_instance.name = "MeshInstance3D"
		mesh_instance.mesh = visual_mesh
		mesh_instance.layers = csg.layers
		mesh_instance.cast_shadow = csg.cast_shadow
		mesh_instance.gi_mode = csg.gi_mode

		static_body.add_child(mesh_instance)

		if collision_shape_resource != null:
			var collision_shape := CollisionShape3D.new()
			collision_shape.name = "TrimeshCollisionShape3D"
			collision_shape.shape = collision_shape_resource
			static_body.add_child(collision_shape)

		baked_scene_root.add_child(static_body)
		baked_count += 1

	if baked_count == 0:
		baked_scene_root.free()
		push_warning("CSG bake completed, but no valid meshes were produced.")
		return

	_set_owner_recursive(baked_scene_root, baked_scene_root)

	var packed_scene := PackedScene.new()
	var pack_error := packed_scene.pack(baked_scene_root)

	if pack_error != OK:
		baked_scene_root.free()
		push_error("PackedScene.pack() failed: %s" % str(pack_error))
		return

	var source_scene_name := _get_source_scene_base_name(edited_root)
	var safe_scene_name := _make_safe_file_name(source_scene_name)
	var output_path := "%s/%s_baked_csg.res" % [
		output_folder.trim_suffix("/"),
		safe_scene_name
	]

	var save_error := ResourceSaver.save(packed_scene, output_path)
	baked_scene_root.free()

	if save_error != OK:
		push_error("ResourceSaver.save() failed for '%s': %s" % [output_path, str(save_error)])
		return

	EditorInterface.get_resource_filesystem().scan()
	EditorInterface.select_file(output_path)

	print("Baked %d CSG root shape(s) into: %s" % [baked_count, output_path])


func _collect_csg_bake_targets(node: Node, bake_targets: Array[CSGShape3D]) -> void:
	if node is CSGShape3D:
		var csg := node as CSGShape3D

		# Only bake root CSG results.
		# This handles CSGCombiner3D correctly:
		# a root combiner gets baked once, and its CSG children are not baked individually.
		if csg.is_root_shape():
			bake_targets.append(csg)

		# Do not recurse into CSG children.
		# Their geometry is already represented by the root CSG result.
		return

	for child in node.get_children():
		_collect_csg_bake_targets(child, bake_targets)


func _set_owner_recursive(node: Node, owner_node: Node) -> void:
	for child in node.get_children():
		child.owner = owner_node
		_set_owner_recursive(child, owner_node)


func _get_source_scene_base_name(edited_root: Node) -> String:
	if not edited_root.scene_file_path.is_empty():
		return edited_root.scene_file_path.get_file().get_basename()

	return str(edited_root.name)


func _make_safe_file_name(value: String) -> String:
	var result := ""

	for i in value.length():
		var c := value[i]

		if c in ['<', '>', ':', '"', '/', '\\', '|', '?', '*']:
			result += "_"
		else:
			result += c

	result = result.strip_edges()

	if result.is_empty():
		return "baked_csg_scene"

	return result


func _make_safe_node_name(value: String) -> String:
	var result := value
	result = result.replace("/", "_")
	result = result.replace(":", "_")
	result = result.replace("@", "_")
	result = result.strip_edges()

	if result.is_empty():
		return "BakedCSG"

	return result
