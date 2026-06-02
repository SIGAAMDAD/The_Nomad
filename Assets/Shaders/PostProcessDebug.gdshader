shader_type canvas_item;
render_mode unshaded;

// Runtime-debug post-process shader.
//
// Height buffer channel contract:
//
// R = normalized per-pixel height
// G = contact / foot AO mask
// B = directional caster silhouette
// A = full visible silhouette, used to avoid shadowing the caster itself

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D height_texture : repeat_disable, filter_linear;

uniform int debug_view : hint_enum(
	"Composited",
	"Height Buffer RGB",
	"Height",
	"Contact Mask",
	"Directional Caster Mask",
	"Visible/Caster Skip Mask",
	"Directional Shadow",
	"Height Shadow",
	"SDF Shadow",
	"Contact AO",
	"Total Darkening"
) = 0;

uniform bool enable_height_shadows = true;

// Keep this false while tuning.
// This SDF path is now height-gated, but the height-buffer path should be the main sunlight shadow.
uniform bool enable_sdf_shadows = false;

// SDF should not be allowed to dominate the height result.
uniform float sdf_shadow_weight : hint_range(0.0, 1.0) = 0.25;

uniform bool enable_contact_ao = true;
uniform bool enable_fog = true;
uniform bool enable_color_grade = true;
uniform bool enable_vignette = true;
uniform bool enable_color_dither = true;

// Do not enable this until the shadow shape is correct.
// Per-pixel ray jitter causes the raked dirt look.
uniform bool enable_shadow_jitter = false;

uniform int shadow_steps : hint_range(1, 128, 1) = 96;
uniform int contact_ao_taps : hint_range(4, 12, 4) = 8;

uniform vec3 shadow_color : source_color = vec3(0.0, 0.0, 0.0);
uniform float shadow_opacity : hint_range(0.0, 1.0) = 0.38;

// Direction shadows travel in screen space.
// Example: vec2(0.0, 1.0) means shadows extend downward.
uniform vec2 shadow_direction_screen = vec2(0.0, 1.0);

// 0 = sun near horizon, 1 = sun high overhead.
uniform float sun_height : hint_range(0.0, 1.0) = 0.45;

// Elevation angles in radians.
uniform float min_sun_angle_rad : hint_range(0.02, 1.5) = 0.12;
uniform float max_sun_angle_rad : hint_range(0.02, 1.5) = 1.20;

// Red = 1 in height texture means this many pseudo-pixels tall.
// Must match HeightPassRenderer2D.height_max_px.
uniform float height_max_px : hint_range(1.0, 1024.0) = 256.0;

// Hard cap for shadow length in screen pixels.
uniform float shadow_max_length_px : hint_range(0.0, 2048.0) = 220.0;

// Multiplier after height/elevation projection.
uniform float shadow_length_multiplier : hint_range(0.0, 4.0) = 1.0;

// Softness for heightfield shadows.
uniform float height_shadow_softness_px : hint_range(0.1, 128.0) = 24.0;

// Fade shadow near its computed end.
uniform float shadow_tail_fade_px : hint_range(0.0, 512.0) = 96.0;

// Prevent projected shadow from painting over the caster sprite.
uniform bool keep_occluders_unshadowed = true;

// SDF settings.
// This path is intentionally conservative and height-gated.
uniform float sdf_hit_threshold_px : hint_range(0.01, 8.0) = 0.8;
uniform float sdf_min_step_px : hint_range(0.1, 8.0) = 1.0;
uniform float sdf_max_step_px : hint_range(1.0, 64.0) = 18.0;

// Contact AO.
uniform float contact_ao_radius_px : hint_range(0.0, 64.0) = 10.0;
uniform float contact_ao_strength : hint_range(0.0, 2.0) = 0.55;
uniform float contact_ao_max : hint_range(0.0, 1.0) = 0.35;

// Prevent AO from painting over the caster itself.
uniform bool contact_ao_skip_casters = true;

// Fog/color grade/vignette.
uniform vec3 fog_color : source_color = vec3(0.62, 0.68, 0.78);
uniform float fog_strength : hint_range(0.0, 1.0) = 0.12;
uniform float fog_start_y : hint_range(0.0, 1.0) = 0.15;
uniform float fog_end_y : hint_range(0.0, 1.0) = 1.0;

uniform float exposure : hint_range(0.0, 4.0) = 1.0;
uniform float contrast : hint_range(0.0, 3.0) = 1.06;
uniform float saturation : hint_range(0.0, 3.0) = 1.04;
uniform float gamma_value : hint_range(0.2, 3.0) = 1.0;
uniform vec3 grade_tint : source_color = vec3(1.0, 1.0, 1.0);

uniform float vignette_strength : hint_range(0.0, 1.0) = 0.18;
uniform float vignette_inner : hint_range(0.0, 2.0) = 0.45;
uniform float vignette_outer : hint_range(0.0, 2.0) = 1.15;

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

float sample_height_px(vec2 uv) {
	vec4 h = sample_height_buffer(uv);

	// R already contains the per-pixel normalized height.
	// Do not multiply by A here; the height writer already applied silhouette.
	return h.r * height_max_px;
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

	// This is a ground-shadow pass. Do not darken the caster itself.
	if (keep_occluders_unshadowed && here.a > 0.01) {
		return 0.0;
	}

	vec2 dir = shadow_direction_screen;

	if (length(dir) < 0.0001) {
		return 0.0;
	}

	dir = normalize(dir);

	// Ground receiver for now.
	// If you add terrain height later, replace this with terrain sample height.
	float receiver_height_px = 0.0;

	float slope = safe_sun_slope();
	float step_px = max(1.0, shadow_max_length_px / float(max(shadow_steps, 1)));

	float jitter = 0.5;

	if (enable_shadow_jitter) {
		jitter = hash12(frag_coord.xy);
	}

	float shadow = 0.0;

	for (int i = 0; i < 128; i++) {
		if (i >= shadow_steps) {
			break;
		}

		float dist_px = (float(i) + jitter) * step_px;

		if (dist_px > shadow_max_length_px) {
			break;
		}

		// To know whether this receiver pixel is shadowed,
		// march backward toward the light.
		vec2 sample_uv = uv - dir * dist_px * pixel_size;

		if (
			sample_uv.x < 0.0 || sample_uv.x > 1.0 ||
			sample_uv.y < 0.0 || sample_uv.y > 1.0
		) {
			break;
		}

		vec4 caster_sample = sample_height_buffer(sample_uv);

		// B = directional caster silhouette.
		float caster_mask = caster_sample.b;

		if (caster_mask <= 0.01) {
			continue;
		}

		// R = normalized per-pixel height.
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

		float occlusion = smoothstep(
			-height_shadow_softness_px,
			height_shadow_softness_px,
			height_excess
		);

		occlusion *= caster_mask;
		occlusion *= tail_fade(dist_px, max_len_px);

		shadow = max(shadow, occlusion);
	}

	return clamp(shadow, 0.0, 1.0);
}

float sdf_shadow(vec2 uv) {
	if (!enable_sdf_shadows || shadow_max_length_px <= 0.0) {
		return 0.0;
	}

	vec2 dir = shadow_direction_screen;

	if (length(dir) < 0.0001) {
		return 0.0;
	}

	dir = normalize(dir);

	vec2 sdf_origin = screen_uv_to_sdf(uv);
	float sdf_here = texture_sdf(sdf_origin);

	if (keep_occluders_unshadowed && sdf_here <= 0.0) {
		return 0.0;
	}

	float receiver_height_px = 0.0;
	float slope = safe_sun_slope();

	float t = max(0.5, sdf_min_step_px);
	float shadow = 0.0;

	for (int i = 0; i < 128; i++) {
		if (i >= shadow_steps || t >= shadow_max_length_px) {
			break;
		}

		vec2 sdf_p = sdf_origin - dir * t;
		vec2 hit_uv = sdf_to_screen_uv(sdf_p);

		if (
			hit_uv.x < 0.0 || hit_uv.x > 1.0 ||
			hit_uv.y < 0.0 || hit_uv.y > 1.0
		) {
			break;
		}

		float d = texture_sdf(sdf_p);

		if (d <= sdf_hit_threshold_px) {
			vec4 hb = sample_height_buffer(hit_uv);

			// Critical difference from the old path:
			// No default caster height. If the height buffer does not agree,
			// the SDF hit is ignored.
			float caster_mask = hb.b;
			float caster_height_px = hb.r * height_max_px;

			if (caster_mask <= 0.01 || caster_height_px <= 1.0) {
				t += max(sdf_min_step_px, 1.0);
				continue;
			}

			float max_len_px = finite_length_from_height(caster_height_px, receiver_height_px);

			if (t > max_len_px) {
				return 0.0;
			}

			float required_height_px = receiver_height_px + t * slope;
			float height_excess = caster_height_px - required_height_px;

			float occlusion = smoothstep(
				-height_shadow_softness_px,
				height_shadow_softness_px,
				height_excess
			);

			occlusion *= caster_mask;
			occlusion *= tail_fade(t, max_len_px);

			shadow = max(shadow, occlusion);
			break;
		}

		t += clamp(d, sdf_min_step_px, sdf_max_step_px);
	}

	return clamp(shadow, 0.0, 1.0);
}

float sample_contact_mask(vec2 uv) {
	if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
		return 0.0;
	}

	// G = foot/contact mask from height viewport.
	return texture(height_texture, uv).g;
}

float contact_ao(vec2 uv, vec2 pixel_size) {
	if (!enable_contact_ao) {
		return 0.0;
	}

	vec4 here = texture(height_texture, uv);

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

	// Softer falloff.
	ao = smoothstep(0.0, 1.0, ao);
	ao *= contact_ao_strength;

	return clamp(ao, 0.0, contact_ao_max);
}

vec3 apply_fog(vec3 color, vec2 uv) {
	if (!enable_fog) {
		return color;
	}

	float fog = smoothstep(fog_start_y, fog_end_y, uv.y) * fog_strength;
	return mix(color, fog_color, clamp(fog, 0.0, 1.0));
}

vec3 apply_color_grade(vec3 color) {
	if (!enable_color_grade) {
		return color;
	}

	color *= exposure;
	color = (color - vec3(0.5)) * contrast + vec3(0.5);

	float luma = dot(color, vec3(0.299, 0.587, 0.114));
	color = mix(vec3(luma), color, saturation);

	color *= grade_tint;
	color = pow(max(color, vec3(0.0)), vec3(1.0 / max(gamma_value, 0.001)));

	return color;
}

vec3 apply_vignette(vec3 color, vec2 uv, vec2 pixel_size) {
	if (!enable_vignette || vignette_strength <= 0.0) {
		return color;
	}

	vec2 p = uv * 2.0 - 1.0;

	// Aspect correction from SCREEN_PIXEL_SIZE.
	float aspect = pixel_size.y / max(pixel_size.x, 0.000001);
	p.x *= aspect;

	float dist = length(p);
	float vig = smoothstep(vignette_inner, vignette_outer, dist);

	return color * (1.0 - vig * vignette_strength);
}

vec3 debug_scalar(float value) {
	return vec3(clamp(value, 0.0, 1.0));
}

void fragment() {
	vec4 scene = texture(screen_texture, SCREEN_UV);
	vec4 h = texture(height_texture, SCREEN_UV);

	float height_shadow = heightfield_shadow(SCREEN_UV, SCREEN_PIXEL_SIZE, FRAGCOORD);
	float sdf_shadow_value = sdf_shadow(SCREEN_UV);

	// Height buffer is authoritative.
	// SDF is optional and subordinate.
	float directional_shadow = height_shadow;

	if (enable_sdf_shadows) {
		directional_shadow = max(
			directional_shadow,
			sdf_shadow_value * sdf_shadow_weight
		);
	}

	float ambient_shadow = contact_ao(SCREEN_UV, SCREEN_PIXEL_SIZE);

	float total_darkening = directional_shadow * shadow_opacity;

	// Multiplicative composition avoids overblowing when AO and directional shadow overlap.
	total_darkening = 1.0 - (1.0 - total_darkening) * (1.0 - ambient_shadow);
	total_darkening = clamp(total_darkening, 0.0, 1.0);

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
		COLOR = vec4(debug_scalar(directional_shadow), 1.0);
		discard;
	}

	if (debug_view == 7) {
		COLOR = vec4(debug_scalar(height_shadow), 1.0);
		discard;
	}

	if (debug_view == 8) {
		COLOR = vec4(debug_scalar(sdf_shadow_value), 1.0);
		discard;
	}

	if (debug_view == 9) {
		COLOR = vec4(debug_scalar(ambient_shadow), 1.0);
		discard;
	}

	if (debug_view == 10) {
		COLOR = vec4(debug_scalar(total_darkening), 1.0);
		discard;
	}

	vec3 color = mix(scene.rgb, shadow_color, total_darkening);
	color = apply_fog(color, SCREEN_UV);
	color = apply_color_grade(color);
	color = apply_vignette(color, SCREEN_UV, SCREEN_PIXEL_SIZE);

	if (enable_color_dither) {
		color += (hash12(FRAGCOORD.xy) - 0.5) / 255.0;
	}

	COLOR = vec4(color, scene.a);
}
