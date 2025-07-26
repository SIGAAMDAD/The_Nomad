# BloodStainManager.gd
extends MeshInstance2D

const MAX_STAINS = 50
const STAIN_SIZE = 16  # Pixel size of your blood decal

var stain_count = 0
var surface_tool = SurfaceTool.new()
var mesh_data: ArrayMesh
var stain_texture: Texture2D

func _ready():
	stain_texture = preload("res://textures/blood1.png")
	mesh_data = ArrayMesh.new()
	
	# Create initial empty mesh
	surface_tool.begin(Mesh.PRIMITIVE_TRIANGLES)
	surface_tool.set_material(StandardMaterial3D.new())
	surface_tool.commit(mesh_data)
	mesh = mesh_data

func add_stain(position: Vector2, size_variation: float = 1.0):
	# Create new quad at position
	var half_size = STAIN_SIZE * 0.5 * size_variation
	var vertices = PackedVector2Array([
		position + Vector2(-half_size, -half_size),
		position + Vector2(-half_size, half_size),
		position + Vector2(half_size, half_size),
		position + Vector2(half_size, -half_size)
	])
	
	# Create new surface for each stain (simplest for 2D)
	var new_mesh = ArrayMesh.new()
	surface_tool.begin(Mesh.PRIMITIVE_TRIANGLES)
	
	# Add vertices (two triangles)
	var vertexes0: PackedVector3Array = [
		Vector3(vertices[0].x, vertices[0].y, 0),
		Vector3(vertices[1].x, vertices[1].y, 0),
		Vector3(vertices[2].x, vertices[2].y, 0)
	]
	var uvs0: PackedVector2Array = [
		Vector2(0, 0),
		Vector2(0, 1),
		Vector2(1, 1)
	]
	
	surface_tool.add_triangle_fan( vertexes0, uvs0 )
	
	var vertexes1:PackedVector3Array = [
		Vector3(vertices[0].x, vertices[0].y, 0),
		Vector3(vertices[2].x, vertices[2].y, 0),
		Vector3(vertices[3].x, vertices[3].y, 0)
	]
	var uvs1:PackedVector2Array = [
		Vector2(0, 0),
		Vector2(1, 1),
		Vector2(1, 0)
	]
	
	surface_tool.add_triangle_fan( vertexes1, uvs1 )
	
	surface_tool.generate_normals()
	surface_tool.commit(new_mesh)
	
	# Create material
	var mat = StandardMaterial3D.new()
	mat.albedo_texture = stain_texture
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA_SCISSOR
	mat.albedo_color = Color(0.7, 0, 0, 0.9)
	mat.params_cull_mode = StandardMaterial3D.CULL_DISABLED
	mat.shading_mode = StandardMaterial3D.SHADING_MODE_UNSHADED
	new_mesh.surface_set_material(0, mat)
	
	# Add as new surface to existing mesh
	for surface_idx in range(mesh_data.get_surface_count()):
		mesh_data.surface_remove(surface_idx)
	
	mesh_data = new_mesh
	mesh = mesh_data
	stain_count += 1
	
	# Remove oldest stain if over limit
	if stain_count > MAX_STAINS:
		# For simplicity, we're replacing the whole mesh
		# For better performance, implement a ring buffer
		stain_count = MAX_STAINS
