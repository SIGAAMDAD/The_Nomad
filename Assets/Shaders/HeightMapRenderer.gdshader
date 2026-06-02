shader_type canvas_item;
render_mode unshaded, blend_disabled;

// Packed from HeightPassRenderer2D.draw_texture_rect_region(..., packed_params)
//
// params.r = normalized full object height, 0..1
// params.g = contact band UV height, usually around 0.05..0.15
// params.b = unused
// params.a = source canvas alpha/modulate alpha
varying vec4 params;

// Pixel-art sprites often have fuzzy antialiasing/padding.
// Tighten this if transparent crumbs are still casting.
uniform float alpha_low : hint_range(0.0, 1.0) = 0.35;
uniform float alpha_high : hint_range(0.0, 1.0) = 0.65;

// Shape of the vertical height ramp.
// < 1.0 keeps lower pixels taller.
// > 1.0 makes most height concentrate near the top.
uniform float height_curve_power : hint_range(0.1, 4.0) = 0.75;

// Prevent the lower body/base from becoming exactly zero-height.
// This helps large pixel-art structures avoid curtain-looking shadows.
uniform float min_body_height : hint_range(0.0, 0.5) = 0.12;

void vertex() {
	params = COLOR;
}

void fragment() {
	float tex_alpha = texture(TEXTURE, UV).a;

	// Stable binary-ish silhouette.
	float silhouette = smoothstep(alpha_low, alpha_high, tex_alpha) * params.a;

	if (silhouette <= 0.001) {
		discard;
	}

	// 1 at top of sprite, 0 at bottom.
	float vertical_height = clamp(1.0 - UV.y, 0.0, 1.0);

	float object_height_norm = clamp(params.r, 0.0, 1.0);
	float contact_band_uv = max(params.g, 0.001);

	// Contact is only for grounding/AO.
	float contact = 1.0 - smoothstep(0.0, contact_band_uv, vertical_height);
	contact *= silhouette;

	// Directional shadow height is full-silhouette, not footprint-only.
	float height_curve = pow(vertical_height, max(height_curve_power, 0.001));
	height_curve = max(height_curve, min_body_height);

	float height = object_height_norm * height_curve * silhouette;

	// Full visible silhouette participates as the directional caster mask.
	float directional_caster = silhouette;

	// Channel contract:
	// R = normalized per-pixel height
	// G = contact / foot AO mask
	// B = directional caster silhouette
	// A = full visible silhouette, used to avoid shadowing the caster itself
	COLOR = vec4(
		height,
		contact,
		directional_caster,
		silhouette
	);
}
