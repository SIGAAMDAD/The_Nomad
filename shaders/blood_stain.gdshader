// stain_buffer.gdshader
shader_type canvas_item;

uniform sampler2D stain_texture;

void fragment() {
	vec4 scene_color = texture(TEXTURE, UV);
	vec4 stain_color = texture(stain_texture, SCREEN_UV);
	
	// Blend with background (darken red channel)
	COLOR = vec4(
		mix(scene_color.rgb, stain_color.rgb, stain_color.a),
		max(scene_color.a, stain_color.a)
	);
}