shader_type canvas_item;
render_mode unshaded;

// Godot 4 screen read.
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// Direction the shadow extends on screen.
// Example: vec2(1.0, 1.0) means shadows cast down-right.
uniform vec2 shadow_direction_screen = vec2(1.0, 1.0);

// Finite shadow length, in screen/SDF-space pixels.
uniform float shadow_length_px : hint_range(0.0, 2048.0) = 160.0;

// Runtime quality. Higher = longer/more accurate raymarch.
// Keep this <= MAX_STEPS below.
uniform int quality : hint_range(1, 128) = 48;

// Soft edge approximation. Higher = softer/wider penumbra.
uniform float softness_px : hint_range(1.0, 512.0) = 80.0;

// Fade near the end of the finite shadow.
uniform float tail_fade_px : hint_range(0.0, 512.0) = 80.0;

// Shadow color and max opacity.
uniform vec4 shadow_tint : source_color = vec4(0.0, 0.0, 0.0, 0.45);

// Raymarch tuning.
uniform float hit_threshold_px : hint_range(0.01, 4.0) = 0.75;
uniform float min_step_px : hint_range(0.1, 8.0) = 1.0;
uniform float max_step_px : hint_range(1.0, 64.0) = 16.0;

// Usually true, so caster sprites/occluder regions do not darken themselves.
uniform bool keep_occluders_unshadowed = true;

// Compile-time cap. The uniform `quality` controls the real runtime cost.
const int MAX_STEPS = 128;

float finite_shadow(vec2 screen_uv) {
	vec2 shadow_dir = shadow_direction_screen;

	if (length(shadow_dir) < 0.0001 || shadow_length_px <= 0.0) {
		return 0.0;
	}

	shadow_dir = normalize(shadow_dir);

	// To decide whether this pixel is in shadow, trace from the pixel
	// back toward the light, opposite the shadow direction.
	vec2 ray_dir = -shadow_dir;

	vec2 sdf_pos = screen_uv_to_sdf(screen_uv);
	float sdf_here = texture_sdf(sdf_pos);

	if (keep_occluders_unshadowed && sdf_here <= 0.0) {
		return 0.0;
	}

	float t = max(min_step_px, 0.5);
	float visibility = 1.0;
	float best_t = shadow_length_px;

	// Bigger softness_px = softer shadow.
	float hardness = max(1.0, shadow_length_px / max(softness_px, 1.0));

	for (int i = 0; i < MAX_STEPS; i++) {
		if (i >= quality || t >= shadow_length_px) {
			break;
		}

		vec2 p = sdf_pos + ray_dir * t;
		float d = texture_sdf(p);

		// Direct hit: this pixel is behind an occluder.
		if (d <= hit_threshold_px) {
			visibility = 0.0;
			best_t = t;
			break;
		}

		// SDF soft-shadow approximation.
		// Close approaches to an occluder reduce visibility near shadow edges.
		float candidate_visibility = clamp(hardness * d / max(t, 1.0), 0.0, 1.0);

		if (candidate_visibility < visibility) {
			visibility = candidate_visibility;
			best_t = t;
		}

		t += clamp(d, min_step_px, max_step_px);
	}

	float shadow_amount = 1.0 - clamp(visibility, 0.0, 1.0);

	// Fade the shadow out near its finite end.
	float fade_start = max(0.0, shadow_length_px - tail_fade_px);
	float tail_fade = 1.0 - smoothstep(fade_start, shadow_length_px, best_t);

	return shadow_amount * tail_fade;
}

void fragment() {
	vec4 scene = textureLod(screen_texture, SCREEN_UV, 0.0);

	float shadow = finite_shadow(SCREEN_UV);
	float alpha = shadow * shadow_tint.a;

	vec3 shaded = mix(scene.rgb, shadow_tint.rgb, alpha);

	COLOR = vec4(shaded, scene.a);
}
