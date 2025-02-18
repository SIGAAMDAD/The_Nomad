shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform float brightness : hint_range(-1, 1) = 0;
uniform sampler2D source_palette;
uniform sampler2D target_palette;

void fragment() {
	vec4 screen_color = texture(screen_texture, SCREEN_UV);
	ivec2 pal_size = textureSize(source_palette, 0);

	float color_diff = 1.0;
	int nearest_swatch = 0;

	for (int swatch=0; swatch < pal_size.x; swatch++) {
		vec4 sampled_color = texelFetch(source_palette, ivec2(swatch,0), 0);
		float new_color_diff = distance(sampled_color, screen_color);

		if (new_color_diff < color_diff) {
			color_diff = new_color_diff;
			nearest_swatch = swatch;
		}
	}

	int swatch_offset = int(brightness * float(pal_size.x));
	int target_swatch = clamp(nearest_swatch + swatch_offset, 0, pal_size.x -1);

	COLOR = texelFetch(target_palette, ivec2(target_swatch, 0), 0);
}