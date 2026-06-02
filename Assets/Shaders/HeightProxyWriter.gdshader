shader_type canvas_item;
render_mode unshaded, blend_disabled;

// Must match WorldFXOverlay/SDFShadows height_max_px.
uniform float height_max_px : hint_range(1.0, 1024.0) = 256.0;

// Usually 1.0.
// Use as broad art-direction class:
// grass/debris = 0.15–0.35
// humanoid = 0.7–1.0
// obelisk/tree/tower = 1.0–2.0
uniform float height_scale : hint_range(0.0, 4.0) = 1.0;

// Bottom fraction of the sprite that contributes to contact AO.
uniform float contact_band_uv : hint_range(0.001, 0.5) = 0.10;

// Sprite-space top and foot.
// For upright sprites, these are usually 0.0 and 1.0.
uniform float top_y_uv : hint_range(-1.0, 1.0) = 0.0;
uniform float foot_y_uv : hint_range(0.0, 2.0) = 1.0;

// Optional manual override.
// Leave at 0.0 to auto-estimate from texture/region height.
uniform float manual_sprite_height_px : hint_range(0.0, 2048.0) = 0.0;

varying float px_per_local_y;
varying vec4 vertex_color;

void vertex() {
	vertex_color = COLOR;

	// Transform one local Y pixel into canvas/screen space.
	vec2 canvas_y = (CANVAS_MATRIX * MODEL_MATRIX * vec4(0.0, 1.0, 0.0, 0.0)).xy;
	px_per_local_y = length(canvas_y);
}

float source_sprite_height_px(vec2 texture_pixel_size, vec4 region_rect) {
	if (manual_sprite_height_px > 0.0) {
		return manual_sprite_height_px;
	}

	float texture_h = 1.0 / max(texture_pixel_size.y, 0.000001);

	// If Sprite2D region is used, REGION_RECT.w is the region height.
	float region_h = region_rect.w;

	return region_h > 0.0 ? region_h : texture_h;
}

void fragment() {
	float tex_alpha = texture(TEXTURE, UV).a;
	float alpha = tex_alpha * vertex_color.a;

	float vertical_height = clamp(
		(foot_y_uv - UV.y) / max(foot_y_uv - top_y_uv, 0.0001),
		0.0,
		1.0
	);

	float object_screen_height_px =
		source_sprite_height_px(TEXTURE_PIXEL_SIZE, REGION_RECT)
		* px_per_local_y
		* height_scale;

	float height_px = object_screen_height_px * vertical_height;
	float height_norm = clamp(height_px / height_max_px, 0.0, 1.0);

	// Foot/contact mask only.
	// This should be narrow. It is for ambient grounding, not long shadows.
	float contact = 1.0 - smoothstep(0.0, contact_band_uv, vertical_height);
	contact *= alpha;

	// R = height.
	// G = contact / foot mask.
	// B = spare.
	// A = caster silhouette.
	COLOR = vec4(height_norm * alpha, contact, 0.0, alpha);
}
