shader_type canvas_item;
render_mode unshaded, blend_disabled;

// Input height buffer channel contract:
// R = normalized per-pixel height
// G = contact / foot AO mask
// B = directional caster silhouette
// A = full visible silhouette, used to avoid shadowing the caster itself
uniform sampler2D height_texture : repeat_disable, filter_linear;

uniform int debug_view : hint_enum(
	"Mask Total",
	"Height Buffer RGB",
	"Height",
	"Contact Mask",
	"Directional Caster Mask",
	"Visible/Caster Skip Mask",
	"Directional Shadow",
	"Contact AO"
) = 0;

uniform bool enable_height_shadows = true;
uniform bool enable_contact_ao = true;
uniform bool enable_shadow_jitter = false;

uniform int shadow_steps : hint_range(1, 128, 1) = 96;
uniform int contact_ao_taps : hint_range(4, 12, 4) = 8;

// Direction shadows travel in screen space.
uniform vec2 shadow_direction_screen = vec2(0.0, 1.0);

// 0 = sun near horizon, 1 = sun high overhead.
uniform float sun_height : hint_range(0.0, 1.0) = 0.45;
uniform float min_sun_angle_rad : hint_range(0.02, 1.5) = 0.12;
uniform float max_sun_angle_rad : hint_range(0.02, 1.5) = 1.20;

// Must match HeightPassRenderer2D.height_max_px.
uniform float height_max_px : hint_range(1.0, 1024.0) = 256.0;

uniform float shadow_max_length_px : hint_range(0.0, 2048.0) = 220.0;
uniform float shadow_length_multiplier : hint_range(0.0, 4.0) = 1.0;
uniform float shadow_softness_px : hint_range(0.1, 128.0) = 24.0;
uniform float shadow_tail_fade_px : hint_range(0.0, 512.0) = 96.0;
uniform float shadow_opacity : hint_range(0.0, 1.0) = 0.38;
uniform bool keep_occluders_unshadowed = true;

uniform float contact_ao_radius_px : hint_range(0.0, 64.0) = 10.0;
uniform float contact_ao_strength : hint_range(0.0, 2.0) = 0.55;
uniform float contact_ao_max : hint_range(0.0, 1.0) = 0.35;
uniform bool contact_ao_skip_casters = true;

float hash12(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

float safe_sun_slope() {
	float h = clamp(sun_height, 0.0, 1.0);
	float angle = mix(min_sun_angle_rad, max_sun_angle_rad, h);
	return max(tan(angle), 0.015);
}

vec4 sample_height_buffer(vec2 uv) {
	if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
		return vec4(0.0);
	}

	return texture(height_texture, uv);
}

float finite_length_from_height(float caster_height_px, float receiver_height_px) {
	float height_delta = max(caster_height_px - receiver_height_px, 0.0);
	float projected = height_delta / safe_sun_slope();
	projected *= shadow_length_multiplier;
	return clamp(projected, 0.0, shadow_max_length_px);
}

float tail_fade(float dist_px, float max_len_px) {
	if (shadow_tail_fade_px <= 0.0) {
		return step(dist_px, max_len_px);
	}

	float start = max(0.0, max_len_px - shadow_tail_fade_px);
	return 1.0 - smoothstep(start, max_len_px, dist_px);
}

float heightfield_shadow(vec2 uv, vec2 pixel_size, vec4 frag_coord) {
	if (!enable_height_shadows || shadow_max_length_px <= 0.0) {
		return 0.0;
	}

	vec4 here = sample_height_buffer(uv);

	if (keep_occluders_unshadowed && here.a > 0.01) {
		return 0.0;
	}

	vec2 dir = shadow_direction_screen;

	if (length(dir) < 0.0001) {
		return 0.0;
	}

	dir = normalize(dir);

	float receiver_height_px = 0.0;
	float slope = safe_sun_slope();
	float step_px = max(1.0, shadow_max_length_px / float(max(shadow_steps, 1)));
	float jitter = enable_shadow_jitter ? hash12(frag_coord.xy) : 0.5;

	float shadow = 0.0;

	for (int i = 0; i < 128; i++) {
		if (i >= shadow_steps) {
			break;
		}

		float dist_px = (float(i) + jitter) * step_px;

		if (dist_px > shadow_max_length_px) {
			break;
		}

		vec2 sample_uv = uv - dir * dist_px * pixel_size;

		if (sample_uv.x < 0.0 || sample_uv.x > 1.0 || sample_uv.y < 0.0 || sample_uv.y > 1.0) {
			break;
		}

		vec4 caster_sample = sample_height_buffer(sample_uv);
		float caster_mask = caster_sample.b;

		if (caster_mask <= 0.01) {
			continue;
		}

		float caster_height_px = caster_sample.r * height_max_px;

		if (caster_height_px <= 1.0) {
			continue;
		}

		float max_len_px = finite_length_from_height(caster_height_px, receiver_height_px);

		if (dist_px > max_len_px) {
			continue;
		}

		float required_height_px = receiver_height_px + dist_px * slope;
		float height_excess = caster_height_px - required_height_px;

		float occlusion = smoothstep(-shadow_softness_px, shadow_softness_px, height_excess);
		occlusion *= caster_mask;
		occlusion *= tail_fade(dist_px, max_len_px);

		shadow = max(shadow, occlusion);
	}

	return clamp(shadow, 0.0, 1.0);
}

float sample_contact_mask(vec2 uv) {
	if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
		return 0.0;
	}

	return texture(height_texture, uv).g;
}

float contact_ao(vec2 uv, vec2 pixel_size) {
	if (!enable_contact_ao) {
		return 0.0;
	}

	vec4 here = sample_height_buffer(uv);

	if (contact_ao_skip_casters && here.a > 0.01) {
		return 0.0;
	}

	vec2 r = pixel_size * contact_ao_radius_px;

	float occ = 0.0;
	float taps = 0.0;

	occ += sample_contact_mask(uv + vec2( 1.0,  0.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2(-1.0,  0.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2( 0.0,  1.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2( 0.0, -1.0) * r); taps += 1.0;

	if (contact_ao_taps >= 8) {
		occ += sample_contact_mask(uv + vec2( 0.70710678,  0.70710678) * r); taps += 1.0;
		occ += sample_contact_mask(uv + vec2(-0.70710678,  0.70710678) * r); taps += 1.0;
		occ += sample_contact_mask(uv + vec2( 0.70710678, -0.70710678) * r); taps += 1.0;
		occ += sample_contact_mask(uv + vec2(-0.70710678, -0.70710678) * r); taps += 1.0;
	}

	if (contact_ao_taps >= 12) {
		occ += sample_contact_mask(uv + vec2( 1.5,  0.0) * r); taps += 1.0;
		occ += sample_contact_mask(uv + vec2(-1.5,  0.0) * r); taps += 1.0;
		occ += sample_contact_mask(uv + vec2( 0.0,  1.5) * r); taps += 1.0;
		occ += sample_contact_mask(uv + vec2( 0.0, -1.5) * r); taps += 1.0;
	}

	float ao = occ / max(taps, 1.0);
	ao = smoothstep(0.0, 1.0, ao);
	ao *= contact_ao_strength;

	return clamp(ao, 0.0, contact_ao_max);
}

vec3 debug_scalar(float value) {
	return vec3(clamp(value, 0.0, 1.0));
}

void fragment() {
	vec4 h = texture(height_texture, UV);
	vec2 height_pixel_size = 1.0 / vec2(textureSize(height_texture, 0));

	float directional = heightfield_shadow(UV, height_pixel_size, FRAGCOORD);
	float ambient = contact_ao(UV, height_pixel_size);

	float total = directional * shadow_opacity;
	total = 1.0 - (1.0 - total) * (1.0 - ambient);
	total = clamp(total, 0.0, 1.0);

	if (debug_view == 1) {
		COLOR = vec4(h.rgb, 1.0);
		discard;
	}
	if (debug_view == 2) {
		COLOR = vec4(debug_scalar(h.r), 1.0);
		discard;
	}
	if (debug_view == 3) {
		COLOR = vec4(debug_scalar(h.g), 1.0);
		discard;
	}
	if (debug_view == 4) {
		COLOR = vec4(debug_scalar(h.b), 1.0);
		discard;
	}
	if (debug_view == 5) {
		COLOR = vec4(debug_scalar(h.a), 1.0);
		discard;
	}
	if (debug_view == 6) {
		COLOR = vec4(debug_scalar(directional), 1.0);
		discard;
	}
	if (debug_view == 7) {
		COLOR = vec4(debug_scalar(ambient), 1.0);
		discard;
	}

	// R = directional raw, G = contact AO, B = total darkening, A = total darkening
	COLOR = vec4(directional, ambient, total, total);
}
