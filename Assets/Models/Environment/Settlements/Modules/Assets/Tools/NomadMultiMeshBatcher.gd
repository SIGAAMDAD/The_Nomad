@tool
extends EditorScript

# Select a root Node3D in the editor, then run this EditorScript.
# It scans all MeshInstance3D descendants and creates MultiMeshInstance3D batches
# for repeated shape + material category groups.
#
# Grouping key:
#   1) mesh.resource_path, so the shape is identical and safe for MultiMesh.
#   2) material category inferred from node name, e.g. Mesh_Mat_WoodBeam_Foo -> Mat_WoodBeam.
#   3) material_override resource path if one is assigned.
#
# This works especially well with the v25 kit because most parts reference shared primitive
# meshes such as UnitBox, UnitCylinder, UnitSphere and encode material intent in node names.

const MIN_INSTANCES_TO_BATCH := 3
const HIDE_ORIGINALS := true
const REMOVE_ORIGINALS := false
const BATCH_ROOT_NAME := "BakedMultiMeshBatches"

func _run() -> void:
    var selected := get_editor_interface().get_selection().get_selected_nodes()
    if selected.is_empty():
        push_error("Select a Node3D root before running NomadMultiMeshBatcher.gd.")
        return
    var root := selected[0] as Node3D
    if root == null:
        push_error("Selected node must be a Node3D.")
        return

    var mesh_instances: Array[MeshInstance3D] = []
    _collect_mesh_instances(root, mesh_instances)

    var groups: Dictionary = {}
    for mi in mesh_instances:
        if mi.mesh == null:
            continue
        if mi.get_parent() != null and String(mi.get_parent().name) == BATCH_ROOT_NAME:
            continue
        var mesh_path := mi.mesh.resource_path
        if mesh_path.is_empty():
            mesh_path = "RID:%s" % str(mi.mesh.get_rid())
        var mat_key := _material_key(mi)
        var key := mesh_path + "||" + mat_key
        if not groups.has(key):
            groups[key] = []
        groups[key].append(mi)

    var batch_root := root.get_node_or_null(BATCH_ROOT_NAME) as Node3D
    if batch_root == null:
        batch_root = Node3D.new()
        batch_root.name = BATCH_ROOT_NAME
        root.add_child(batch_root)
        batch_root.owner = root.owner if root.owner != null else root

    var made := 0
    var root_inv := root.global_transform.affine_inverse()
    for key in groups.keys():
        var list: Array = groups[key]
        if list.size() < MIN_INSTANCES_TO_BATCH:
            continue
        var first := list[0] as MeshInstance3D
        if first == null or first.mesh == null:
            continue

        var mm := MultiMesh.new()
        mm.transform_format = MultiMesh.TRANSFORM_3D
        mm.mesh = first.mesh
        mm.instance_count = list.size()
        for i in range(list.size()):
            var src := list[i] as MeshInstance3D
            mm.set_instance_transform(i, root_inv * src.global_transform)

        var mmi := MultiMeshInstance3D.new()
        mmi.name = _safe_name("MMI_" + _material_key(first) + "_" + first.mesh.resource_path.get_file().get_basename())
        mmi.multimesh = mm
        mmi.transform = Transform3D.IDENTITY
        if first.material_override != null:
            mmi.material_override = first.material_override
        batch_root.add_child(mmi)
        mmi.owner = root.owner if root.owner != null else root
        made += 1

        for src in list:
            var node := src as MeshInstance3D
            if REMOVE_ORIGINALS:
                node.queue_free()
            elif HIDE_ORIGINALS:
                node.visible = false

    print("NomadMultiMeshBatcher: created %d MultiMeshInstance3D batches." % made)

func _collect_mesh_instances(node: Node, out: Array[MeshInstance3D]) -> void:
    if node is MeshInstance3D:
        out.append(node)
    for child in node.get_children():
        _collect_mesh_instances(child, out)

func _material_key(mi: MeshInstance3D) -> String:
    if mi.material_override != null:
        var p := mi.material_override.resource_path
        if not p.is_empty():
            return "MaterialResource_" + p.get_file().get_basename()
    var n := String(mi.name)
    var re := RegEx.new()
    re.compile("Mat_[A-Za-z0-9_]+")
    var m := re.search(n)
    if m != null:
        # Prefer the first two semantic chunks, e.g. Mat_WoodBeam, Mat_BrickWall.
        var raw := m.get_string()
        var parts := raw.split("_")
        if parts.size() >= 3:
            return parts[0] + "_" + parts[1]
        return raw
    return "NoMaterialKey"

func _safe_name(s: String) -> String:
    var re := RegEx.new()
    re.compile("[^A-Za-z0-9_]")
    var out := re.sub(s, "_", true)
    var collapse := RegEx.new()
    collapse.compile("_+")
    out = collapse.sub(out, "_", true)
    out = out.strip_edges(false, false)
    while out.begins_with("_") and out.length() > 1:
        out = out.substr(1)
    if out.is_empty():
        return "_Node"
    var first := out.substr(0, 1)
    var first_ok := RegEx.new()
    first_ok.compile("^[A-Za-z_]$")
    if first_ok.search(first) == null:
        out = "_" + out
    return out
