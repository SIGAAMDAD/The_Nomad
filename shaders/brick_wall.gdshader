/*
Shader from Godot Shaders - the free shader library.
godotshaders.com/shader/brick-tiles

Feel free to improve and change this shader according to your needs
and consider sharing the modified result on godotshaders.com.

Tiling effect from https://thebookofshaders.com/09/
*/

shader_type canvas_item;

uniform float size = 5.0;
uniform float thickness : hint_range (0.0, 0.5) = 0.08;
uniform float symmetry : hint_range(0.0, 2.0) = 1.0;
uniform vec2 ratio = vec2(0.5, 1.0);

uniform vec4 brick_color:source_color = vec4(0.52, 0.28, 0.18, 1.0);
uniform vec4 mortar_color = vec4(0.55, 0.55, 0.55, 1.0);
uniform sampler2D rough_noise; // Add an OpenSimplexNoise and set 'Height' to 200, 'Period' to 15 and 'Lacunarity' to 3
uniform float rough_amount : hint_range(0.0, 2.0) = 1.0;

uniform vec2 scale = vec2(1.0); // Don't edit this, it is used by the script to get the nodes scale.

vec2 brick_tile(vec2 _st, float _zoom)
{
	_st *= _zoom;

	// Here is where the offset is happening
	_st.x += (step(1.0, mod(_st.y, 2.0)) * 0.5) * symmetry;

	return fract(_st);
}

float box(vec2 _st, vec2 _size, vec2 _ratio)
{
	_size = (vec2(0.5, 0.5) * _size * _ratio);
	vec2 uv = smoothstep(_size, _size + vec2(1e-4), _st);
	uv *= smoothstep(_size, _size + vec2(1e-4), (vec2(1.0) - _st));
	return uv.x * uv.y;
}

vec3 uneven(vec2 _st, vec2 _uv, float _rough_amount)
{
	// This function works by practically making the lines again but with smooth edges
	// and multiplying a noise texture to it. Add an OpenSimplexNoise to 'rough_noise'.
	// Adjust the values or make your own function completely and play with the values, both in this
	// function but also in the noise texture.
	//
	// In the Noise texture, start with setting 'Period' to 15 and 'Lacunarity' to 3.

	vec2 _size = vec2(_rough_amount) * ratio;
	vec2 uv = smoothstep(vec2(0.0), _size , _st);
	uv *= smoothstep(vec2(0.0), _size , (vec2(1.0) - _st));
	vec2 fringe = uv;

	vec3 rough_text = texture(rough_noise, _uv).rgb;
	rough_text += vec3(fringe.r);
	rough_text = smoothstep(0.35, 0.4, rough_text);

	return rough_text;
}

void fragment()
{
	// To make the pattern keep its proportions even when resizing the texture,
	// add a script with this code to the node:
	/*
	tool
	extends Sprite

	func _ready():
		set_notify_transform(true)

	func _notification(what):
		if what == NOTIFICATION_TRANSFORM_CHANGED:
			get_material().set_shader_param("scale", scale)
	*/

	// Set the brick shape
	vec2 shape_uv = UV * scale * ratio;

	// Create the brick tiling
	shape_uv = brick_tile(shape_uv, size);

	// Apply some grungy uneveness. You can play around with the values in the 'uneven' function for other results
	vec3 edge = vec3(uneven(shape_uv, UV, rough_amount));

	// Create the "mortar" lines
	vec3 color = vec3(box(shape_uv, vec2(thickness), ratio)) * vec3(edge);

	// Apply the colors
	color = mix(mortar_color.rgb, brick_color.rgb, color.r);

	// Uncomment to see the space coordinates. This is quite neat and can give you an idea of how the shader works.
	//color = vec3(shape_uv, 0.0);

	COLOR = vec4(color, 1.0);
}