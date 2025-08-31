# brush_effect.gd
extends CanvasLayer

@export var outline_size: float = 1.0
@export var outline_color: Color = Color(0.08, 0.05, 0.03)
@export var brush_scale: float = 8.0
@export var brush_edge_variation: float = 0.5
@export var interior_ink_variation: float = 0.3
@export var brush_tex: NoiseTexture2D

var shader_material: ShaderMaterial

func _ready():
	# Create full-screen ColorRect
	var rect = ColorRect.new()
	rect.anchor_right = 1.0
	rect.anchor_bottom = 1.0
	rect.set_anchors_preset( Control.PRESET_FULL_RECT )
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(rect)
	
	# Create shader material
	shader_material = ShaderMaterial.new()
	shader_material.shader = preload("res://shaders/post_processing/brush_effect.gdshader")
	rect.material = shader_material
	
	# Set initial parameters
	update_parameters()
	
func update_parameters():
	shader_material.set_shader_parameter("outline_size", outline_size)
	shader_material.set_shader_parameter("outline_color", outline_color)
	shader_material.set_shader_parameter("brush_scale", brush_scale)
	shader_material.set_shader_parameter("brush_edge_variation", brush_edge_variation)
	shader_material.set_shader_parameter("interior_ink_variation", interior_ink_variation)
	
	# Load brush texture
	shader_material.set_shader_parameter("brush_texture", brush_tex)
