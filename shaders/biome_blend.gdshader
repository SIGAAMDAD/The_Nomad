shader_type canvas_item;

uniform sampler2D biome1_texture;
uniform sampler2D biome2_texture;
uniform bool vertical = false;

// world units per pixel
uniform float texture_scale = 0.01;

void fragment() {
	vec2 world_uv = UV * texture_scale;
	
	vec4 color1 = texture( biome1_texture, world_uv );
	vec4 color2 = texture( biome2_texture, world_uv );
	
	float blend_factor = vertical ? UV.x : UV.y;
	
	blend_factor = smoothstep( 0.0, 1.0, blend_factor );
	
	COLOR = mix( color1, color2, blend_factor );
}