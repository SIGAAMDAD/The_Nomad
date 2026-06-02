#ifndef SDF_SHADOWS_GDSHADERINC
#define SDF_SHADOWS_GDSHADERINC

#if FX_HEIGHT_SHADOWS || FX_SDF_SHADOWS

// ------------------------------------------------------------
// Sun / shadow controls
// ------------------------------------------------------------

// Direction shadows travel in screen space.
// Example: vec2(1.0, 1.0) means shadows extend down-right.
uniform vec2 shadow_direction_screen = vec2(1.0, 1.0);

// 0 = sun near horizon, 1 = sun high overhead.
uniform float sun_height : hint_range(0.0, 1.0) = 0.35;

// Elevation angles in radians.
uniform float min_sun_angle_rad : hint_range(0.02, 1.5) = 0.12;
uniform float max_sun_angle_rad : hint_range(0.02, 1.5) = 1.20;

// Red = 1 in height texture means this many pseudo-pixels tall.
// Must match the height writer's height_max_px.
uniform float height_max_px : hint_range(1.0, 1024.0) = 256.0;

// Hard cap for shadow length in screen pixels.
uniform float shadow_max_length_px : hint_range(0.0, 2048.0) = 320.0;

// Multiplier after height/elevation projection.
uniform float shadow_length_multiplier : hint_range(0.0, 4.0) = 1.0;

// Softness for heightfield shadows.
uniform float height_shadow_softness_px : hint_range(0.1, 128.0) = 10.0;

// Fade shadow near max length.
uniform float shadow_tail_fade_px : hint_range(0.0, 512.0) = 128.0;

// Prevent projected shadow from painting over the caster sprite.
uniform bool keep_occluders_unshadowed = true;

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
	return h.r * h.a * height_max_px;
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

#endif

// ------------------------------------------------------------
// Heightfield shadowing
// ------------------------------------------------------------

#if FX_HEIGHT_SHADOWS

float heightfield_shadow(vec2 uv, vec2 pixel_size, vec4 frag_coord) {
	if (shadow_max_length_px <= 0.0) {
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

	// Receiver is assumed to be ground level.
	// If you later add terrain height, replace this with sample_height_px(uv).
	float receiver_height_px = 0.0;

	float slope = safe_sun_slope();
	float step_px = max(1.0, shadow_max_length_px / float(FX_SHADOW_STEPS));

#if FX_DITHER
	float jitter = hash12(frag_coord.xy);
#else
	float jitter = 0.5;
#endif

	float shadow = 0.0;

	for (int i = 0; i < FX_SHADOW_STEPS; i++) {
		float dist_px = (float(i) + jitter) * step_px;

		if (dist_px > shadow_max_length_px) {
			break;
		}

		// To know whether this receiver pixel is shadowed,
		// march backward toward the light.
		vec2 sample_uv = uv - dir * dist_px * pixel_size;

		if (sample_uv.x < 0.0 || sample_uv.x > 1.0 || sample_uv.y < 0.0 || sample_uv.y > 1.0) {
			break;
		}

		vec4 caster_sample = sample_height_buffer(sample_uv);
		float caster_height_px = caster_sample.r * caster_sample.a * height_max_px;

		float required_height_px = receiver_height_px + dist_px * slope;
		float height_excess = caster_height_px - required_height_px;

		float occlusion = smoothstep(0.0, height_shadow_softness_px, height_excess);
		occlusion *= caster_sample.a;
		occlusion *= tail_fade(dist_px, shadow_max_length_px);

		shadow = max(shadow, occlusion);
	}

	return clamp(shadow, 0.0, 1.0);
}

#else

float heightfield_shadow(vec2 uv, vec2 pixel_size, vec4 frag_coord) {
	return 0.0;
}

#endif

// ------------------------------------------------------------
// SDF shadowing
// ------------------------------------------------------------

#if FX_SDF_SHADOWS

uniform float sdf_hit_threshold_px : hint_range(0.01, 8.0) = 0.8;
uniform float sdf_min_step_px : hint_range(0.1, 8.0) = 1.0;
uniform float sdf_max_step_px : hint_range(1.0, 64.0) = 18.0;
uniform float sdf_softness_px : hint_range(1.0, 512.0) = 90.0;
uniform float sdf_default_caster_height_px : hint_range(0.0, 512.0) = 64.0;

float sdf_shadow(vec2 uv) {
	if (shadow_max_length_px <= 0.0) {
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

	float t = max(0.5, sdf_min_step_px);
	float visibility = 1.0;
	float best_t = shadow_max_length_px;
	float actual_max_len = shadow_max_length_px;

	float hardness = max(1.0, shadow_max_length_px / max(sdf_softness_px, 1.0));

	for (int i = 0; i < FX_SHADOW_STEPS; i++) {
		if (t >= shadow_max_length_px) {
			break;
		}

		vec2 sdf_p = sdf_origin - dir * t;
		vec2 hit_uv = sdf_to_screen_uv(sdf_p);

		if (hit_uv.x < 0.0 || hit_uv.x > 1.0 || hit_uv.y < 0.0 || hit_uv.y > 1.0) {
			break;
		}

		float d = texture_sdf(sdf_p);

		float candidate_visibility = clamp(hardness * d / max(t, 1.0), 0.0, 1.0);

		if (candidate_visibility < visibility) {
			visibility = candidate_visibility;
			best_t = t;
		}

		if (d <= sdf_hit_threshold_px) {
			float sampled_caster_height_px = sample_height_px(hit_uv);
			float caster_height_px = max(sampled_caster_height_px, sdf_default_caster_height_px);

			actual_max_len = finite_length_from_height(caster_height_px, receiver_height_px);

			if (actual_max_len <= 0.0 || t > actual_max_len) {
				return 0.0;
			}

			return tail_fade(t, actual_max_len);
		}

		t += clamp(d, sdf_min_step_px, sdf_max_step_px);
	}

	float shadow = 1.0 - clamp(visibility, 0.0, 1.0);
	shadow *= tail_fade(best_t, actual_max_len);

	return clamp(shadow, 0.0, 1.0);
}

#else

float sdf_shadow(vec2 uv) {
	return 0.0;
}

#endif

#endif
