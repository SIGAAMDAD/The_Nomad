shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture, repeat_disable, filter_linear;

uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform float intensity : hint_range(0.0, 2.0) = 1.0;

// Overall look controls
uniform float chromatic_aberration : hint_range(0.0, 0.05) = 0.012;
uniform float line_jitter_amount : hint_range(0.0, 0.1) = 0.02;
uniform float block_glitch_amount : hint_range(0.0, 0.1) = 0.03;
uniform float scanline_strength : hint_range(0.0, 1.0) = 0.18;
uniform float noise_strength : hint_range(0.0, 1.0) = 0.08;
uniform float vignette_strength : hint_range(0.0, 1.0) = 0.35;
uniform float white_flash_strength : hint_range(0.0, 1.0) = 0.12;

// Pause tint
uniform vec3 pause_tint : source_color = vec3(0.82, 0.9, 1.0);
uniform float tint_strength : hint_range(0.0, 1.0) = 0.12;

// Optional pixelation during peak glitch
uniform float pixelation : hint_range(0.0, 8.0) = 1.5;

// Controls where the effect peaks.
// 0.5 is symmetrical. Push lower/higher if you want it to hit earlier/later.
uniform float peak_position : hint_range(0.0, 1.0) = 0.5;

// If true, effect is heavier around the middle of the animation.
// If false, it ramps linearly with progress.
uniform bool use_peak_curve = true;

float hash(float n) {
	return fract(sin(n) * 43758.5453123);
}

float hash2(vec2 p) {
	return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);

	float a = hash2(i);
	float b = hash2(i + vec2(1.0, 0.0));
	float c = hash2(i + vec2(0.0, 1.0));
	float d = hash2(i + vec2(1.0, 1.0));

	vec2 u = f * f * (3.0 - 2.0 * f);

	return mix(a, b, u.x)
		+ (c - a) * u.y * (1.0 - u.x)
		+ (d - b) * u.x * u.y;
}

float peak_curve(float t, float peak_pos) {
	// Triangle-like envelope with smooth falloff
	float left = smoothstep(0.0, peak_pos, t);
	float right = 1.0 - smoothstep(peak_pos, 1.0, t);
	return left * right * 4.0;
}

vec2 pixelate_uv(vec2 uv, vec2 screen_size, float amount) {
	if (amount <= 0.001) {
		return uv;
	}
	vec2 px = max(vec2(1.0), floor(screen_size / max(vec2(160.0 / amount), 1.0)));
	return floor(uv * px) / px;
}

void fragment() {
	vec2 uv = SCREEN_UV;
	vec2 screen_size = 1.0 / SCREEN_PIXEL_SIZE;
	float time = TIME;

	float t = clamp(progress, 0.0, 1.0);
	float env = use_peak_curve ? peak_curve(t, peak_position) : t;
	env = clamp(env, 0.0, 1.0) * intensity;

	// Slight temporal instability, strongest near peak
	float flicker = 0.92 + 0.08 * sin(time * 80.0 + uv.y * 120.0);

	// Pixelation at peak
	vec2 base_uv = pixelate_uv(uv, screen_size, pixelation * env);

	// Horizontal line tearing
	float line_band = floor(base_uv.y * 120.0);
	float line_rand = hash(line_band + floor(time * 24.0));
	float line_mask = step(0.72, line_rand) * env;
	float line_offset = (line_rand - 0.5) * line_jitter_amount * line_mask;

	// Additional finer jitter
	float fine_jitter = (noise(vec2(time * 18.0, base_uv.y * 220.0)) - 0.5)
		* 0.006 * env;

	// Block glitching
	vec2 block_grid = vec2(26.0, 14.0);
	vec2 block_id = floor(base_uv * block_grid + vec2(floor(time * 14.0), 0.0));
	float block_rand = hash2(block_id);
	float block_mask = step(0.82, block_rand) * env;
	vec2 block_offset = vec2(
		(block_rand - 0.5) * block_glitch_amount,
		(hash(block_rand * 31.7) - 0.5) * block_glitch_amount * 0.25
	) * block_mask;

	vec2 distort_uv = base_uv;
	distort_uv.x += line_offset + fine_jitter;
	distort_uv += block_offset;

	// Barrel-ish instability toward the middle for that "screen stress" feel
	vec2 centered = uv * 2.0 - 1.0;
	float radial = dot(centered, centered);
	distort_uv += centered * radial * 0.01 * env;

	// RGB split
	vec2 ca_dir = vec2(1.0, 0.25);
	ca_dir *= chromatic_aberration * env * flicker;

	vec3 col;
	col.r = texture(screen_tex, distort_uv + ca_dir).r;
	col.g = texture(screen_tex, distort_uv).g;
	col.b = texture(screen_tex, distort_uv - ca_dir).b;

	// Occasional channel dropout blocks
	float dropout = step(0.91, hash2(block_id + 13.7)) * env;
	if (dropout > 0.0) {
		float mode = hash2(block_id + 99.1);
		if (mode < 0.33) {
			col.r *= 0.15;
		} else if (mode < 0.66) {
			col.g *= 0.15;
		} else {
			col.b *= 0.15;
		}
	}

	// Scanlines
	float scan = sin(base_uv.y * screen_size.y * 1.15) * 0.5 + 0.5;
	float scan_mod = mix(1.0, 0.78 + 0.22 * scan, scanline_strength * env);
	col *= scan_mod;

	// High-frequency noise
	float grain = noise(base_uv * screen_size * 0.55 + vec2(time * 45.0, time * 27.0));
	col += (grain - 0.5) * noise_strength * env;

	// Slight cold pause tint
	col = mix(col, col * pause_tint, tint_strength * t);

	// White interference flash near peak
	float peak_flash = smoothstep(0.65, 1.0, env) * (0.5 + 0.5 * sin(time * 90.0));
	col += peak_flash * white_flash_strength * env;

	// Vignette
	float vig = smoothstep(1.15, 0.25, length(centered));
	col *= mix(1.0, vig, vignette_strength * t);

	COLOR = vec4(col, 1.0);
}