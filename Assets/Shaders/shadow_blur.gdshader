shader_type canvas_item;
render_mode unshaded, blend_disabled;

uniform sampler2D source_texture : repeat_disable, filter_linear;

// Use vec2(1, 0) for horizontal, vec2(0, 1) for vertical.
uniform vec2 direction = vec2(1.0, 0.0);
uniform float radius_px : hint_range(0.0, 16.0) = 1.0;

void fragment() {
	vec2 source_pixel_size = 1.0 / vec2(textureSize(source_texture, 0));
	vec2 step_uv = source_pixel_size * direction * radius_px;

	vec4 c = texture(source_texture, UV) * 0.22702703;
	c += texture(source_texture, UV + step_uv * 1.38461538) * 0.31621622;
	c += texture(source_texture, UV - step_uv * 1.38461538) * 0.31621622;
	c += texture(source_texture, UV + step_uv * 3.23076923) * 0.07027027;
	c += texture(source_texture, UV - step_uv * 3.23076923) * 0.07027027;

	COLOR = c;
}
