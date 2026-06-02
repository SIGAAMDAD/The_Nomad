shader_type canvas_item;
render_mode unshaded, blend_mix;

// Full-screen 2D weather overlay for a ColorRect on a CanvasLayer.
// Intended to sit above world art/post color grade but below UI.
// Effects are procedural and cheap enough for early access use.

uniform bool enable_wind = true;
uniform bool enable_rain = false;
uniform bool enable_snow = false;
uniform bool enable_mud = false;

// Global multiplier for the whole weather overlay.
uniform float weather_opacity : hint_range(0.0, 1.0) = 1.0;

// Direction in screen/world space. Example vec2(1, 0) means left-to-right wind.
uniform vec2 wind_direction = vec2(1.0, 0.15);
uniform float wind_speed_px : hint_range(0.0, 3000.0) = 420.0;

// Passed by WeatherOverlayController2D.
uniform vec2 viewport_size = vec2(320.0, 180.0);
uniform vec2 camera_world_center = vec2(0.0, 0.0);
uniform vec2 camera_zoom = vec2(1.0, 1.0);

// Wind/dust streaks.
uniform vec4 wind_color : source_color = vec4(0.82, 0.55, 0.34, 1.0);
uniform float wind_intensity : hint_range(0.0, 2.0) = 0.35;
uniform float wind_density : hint_range(0.0, 1.0) = 0.33;
uniform float wind_streak_length_px : hint_range(16.0, 900.0) = 280.0;
uniform float wind_band_height_px : hint_range(4.0, 160.0) = 30.0;
uniform float wind_thickness : hint_range(0.001, 0.35) = 0.055;
uniform float wind_horizon_bias : hint_range(0.0, 2.0) = 0.35;

// Rain streaks.
uniform vec4 rain_color : source_color = vec4(0.62, 0.72, 0.82, 1.0);
uniform float rain_intensity : hint_range(0.0, 2.0) = 0.75;
uniform float rain_density : hint_range(0.0, 1.0) = 0.45;
uniform float rain_speed_px : hint_range(0.0, 4000.0) = 1050.0;
uniform float rain_cell_width_px : hint_range(4.0, 120.0) = 22.0;
uniform float rain_cell_height_px : hint_range(8.0, 260.0) = 92.0;
uniform float rain_streak_length : hint_range(0.02, 1.0) = 0.55;
uniform float rain_streak_width : hint_range(0.001, 0.15) = 0.035;

// Snow flakes.
uniform vec4 snow_color : source_color = vec4(0.90, 0.94, 1.00, 1.0);
uniform float snow_intensity : hint_range(0.0, 2.0) = 0.65;
uniform float snow_density : hint_range(0.0, 1.0) = 0.38;
uniform float snow_speed_px : hint_range(0.0, 1000.0) = 110.0;
uniform float snow_cell_px : hint_range(8.0, 160.0) = 42.0;
uniform float snow_size_px : hint_range(0.5, 12.0) = 2.0;
uniform float snow_wobble_px : hint_range(0.0, 80.0) = 13.0;

// Mud is a screen/world-space dirty wet film/splatter overlay.
// Actual mud footprints should use weather_footstep_decal.gdshader.
uniform vec4 mud_color : source_color = vec4(0.16, 0.08, 0.035, 1.0);
uniform vec4 mud_highlight_color : source_color = vec4(0.42, 0.23, 0.10, 1.0);
uniform float mud_intensity : hint_range(0.0, 2.0) = 0.35;
uniform float mud_splatter_density : hint_range(0.0, 1.0) = 0.22;
uniform float mud_cell_px : hint_range(8.0, 180.0) = 52.0;
uniform float mud_spot_radius_px : hint_range(1.0, 28.0) = 8.0;
uniform float mud_bottom_bias : hint_range(0.0, 4.0) = 1.5;

// Optional quantization to make the procedural overlay less smooth.
uniform bool pixel_snap = true;
uniform float virtual_pixel_size : hint_range(1.0, 16.0) = 1.0;

float hash12(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

vec2 hash22(vec2 p) {
	return vec2(hash12(p), hash12(p + 37.17));
}

vec2 safe_normalize(vec2 v, vec2 fallback) {
	float len = length(v);
	if (len < 0.0001) {
		return normalize(fallback);
	}
	return v / len;
}

vec2 quantize_px(vec2 px) {
	if (!pixel_snap || virtual_pixel_size <= 1.0) {
		return px;
	}
	return floor(px / virtual_pixel_size) * virtual_pixel_size;
}

vec2 weather_basis_coord(vec2 px, vec2 dir) {
	vec2 d = safe_normalize(dir, vec2(1.0, 0.0));
	vec2 p = vec2(-d.y, d.x);
	return vec2(dot(px, p), dot(px, d));
}

float rain_layer(vec2 screen_px, vec2 dir, float seed, float scale_mul, float speed_mul) {
	vec2 p = weather_basis_coord(screen_px, dir);
	p.y += TIME * rain_speed_px * speed_mul;

	vec2 cell_size = vec2(rain_cell_width_px, rain_cell_height_px) * scale_mul;
	vec2 grid = p / max(cell_size, vec2(1.0));
	vec2 cell = floor(grid);
	vec2 f = fract(grid);

	float rnd = hash12(cell + seed);
	if (rnd > rain_density) {
		return 0.0;
	}

	float drop_x = hash12(cell + seed + 11.7);
	float drop_y = hash12(cell + seed + 71.9);

	float dx = abs(f.x - drop_x);
	dx = min(dx, 1.0 - dx);

	float y = fract(f.y + drop_y);
	float dy = abs(y - 0.5);

	float line = smoothstep(rain_streak_width, 0.0, dx);
	float body = smoothstep(0.5, 0.5 - rain_streak_length * 0.5, dy);
	float tail = 1.0 - smoothstep(0.0, 0.5, y);

	return line * body * mix(0.55, 1.0, rnd) * mix(0.7, 1.0, tail);
}

float snow_layer(vec2 screen_px, vec2 dir, float seed, float scale_mul, float speed_mul) {
	vec2 d = safe_normalize(dir, vec2(1.0, 0.0));
	vec2 p = screen_px;

	p.x += d.x * TIME * snow_speed_px * 0.65 * speed_mul;
	p.y += TIME * snow_speed_px * speed_mul;

	vec2 grid = p / max(snow_cell_px * scale_mul, 1.0);
	vec2 cell = floor(grid);
	vec2 f = fract(grid);

	float rnd = hash12(cell + seed);
	if (rnd > snow_density) {
		return 0.0;
	}

	vec2 center = hash22(cell + seed + 5.0);
	center.x += sin(TIME * mix(0.7, 1.7, rnd) + rnd * 6.28318) * snow_wobble_px / max(snow_cell_px * scale_mul, 1.0);
	center = fract(center);

	float radius = snow_size_px * mix(0.55, 1.55, hash12(cell + seed + 17.0));
	float dist_px = length((f - center) * snow_cell_px * scale_mul);

	float flake = smoothstep(radius, 0.0, dist_px);
	return flake * mix(0.45, 1.0, rnd);
}

float wind_layer(vec2 world_px, vec2 screen_uv, vec2 dir, float seed, float scale_mul, float speed_mul) {
	vec2 p = weather_basis_coord(world_px, dir);
	p.y -= TIME * wind_speed_px * speed_mul;

	vec2 cell_size = vec2(wind_band_height_px, wind_streak_length_px) * scale_mul;
	vec2 grid = p / max(cell_size, vec2(1.0));
	vec2 cell = floor(grid);
	vec2 f = fract(grid);

	float rnd = hash12(cell + seed);
	if (rnd > wind_density) {
		return 0.0;
	}

	float line_y = hash12(cell + seed + 9.0);
	float dy = abs(f.x - line_y);
	float band = smoothstep(wind_thickness, 0.0, dy);

	float segment_center = hash12(cell + seed + 21.0);
	float dx = abs(f.y - segment_center);
	float segment = smoothstep(0.5, 0.08, dx);

	// Slightly stronger toward upper screen if desired, useful for horizon dust.
	float horizon = mix(1.0, 1.0 - screen_uv.y, wind_horizon_bias);
	return band * segment * horizon * mix(0.35, 1.0, rnd);
}

float mud_spatter_layer(vec2 world_px, vec2 screen_uv, float seed, float scale_mul) {
	vec2 grid = world_px / max(mud_cell_px * scale_mul, 1.0);
	vec2 cell = floor(grid);
	vec2 f = fract(grid);

	float rnd = hash12(cell + seed);
	if (rnd > mud_splatter_density) {
		return 0.0;
	}

	vec2 center = hash22(cell + seed + 44.0);
	float radius = mud_spot_radius_px * mix(0.35, 1.35, hash12(cell + seed + 91.0));
	float dist_px = length((f - center) * mud_cell_px * scale_mul);

	float spot = smoothstep(radius, 0.0, dist_px);
	float edge = smoothstep(radius * 1.25, radius * 0.35, dist_px);
	float bottom = pow(clamp(screen_uv.y, 0.0, 1.0), mud_bottom_bias);
	return max(spot, edge * 0.35) * bottom * mix(0.45, 1.0, rnd);
}

void fragment() {
	vec2 screen_px = quantize_px(SCREEN_UV * viewport_size);
	vec2 world_px = camera_world_center + (SCREEN_UV - vec2(0.5)) * viewport_size * camera_zoom;
	world_px = quantize_px(world_px);

	vec2 dir = safe_normalize(wind_direction, vec2(1.0, 0.15));
	vec2 rain_dir = safe_normalize(vec2(dir.x * 0.35, 1.0), vec2(0.1, 1.0));

	vec3 color = vec3(0.0);
	float alpha = 0.0;

	if (enable_wind) {
		float w = 0.0;
		w += wind_layer(world_px, SCREEN_UV, dir, 10.0, 1.00, 1.00);
		w += wind_layer(world_px + vec2(31.0, -17.0), SCREEN_UV, dir, 20.0, 0.55, 1.45) * 0.55;
		w = clamp(w * wind_intensity, 0.0, 1.0);
		color = mix(color, wind_color.rgb, w * wind_color.a);
		alpha = max(alpha, w * wind_color.a);
	}

	if (enable_rain) {
		float r = 0.0;
		r += rain_layer(screen_px, rain_dir, 100.0, 1.00, 1.00);
		r += rain_layer(screen_px + vec2(19.0, 43.0), rain_dir, 130.0, 1.75, 0.72) * 0.65;
		r = clamp(r * rain_intensity, 0.0, 1.0);
		color = mix(color, rain_color.rgb, r * rain_color.a);
		alpha = max(alpha, r * rain_color.a);
	}

	if (enable_snow) {
		float s = 0.0;
		s += snow_layer(screen_px, dir, 200.0, 1.00, 1.00);
		s += snow_layer(screen_px + vec2(71.0, 23.0), dir, 260.0, 1.85, 0.55) * 0.7;
		s = clamp(s * snow_intensity, 0.0, 1.0);
		color = mix(color, snow_color.rgb, s * snow_color.a);
		alpha = max(alpha, s * snow_color.a);
	}

	if (enable_mud) {
		float m = 0.0;
		m += mud_spatter_layer(world_px, SCREEN_UV, 300.0, 1.00);
		m += mud_spatter_layer(world_px + vec2(-43.0, 31.0), SCREEN_UV, 360.0, 0.55) * 0.45;
		m = clamp(m * mud_intensity, 0.0, 1.0);
		vec3 mud_mix = mix(mud_color.rgb, mud_highlight_color.rgb, smoothstep(0.65, 1.0, m) * 0.25);
		color = mix(color, mud_mix, m * mud_color.a);
		alpha = max(alpha, m * mud_color.a);
	}

	alpha *= weather_opacity;
	COLOR = vec4(color, alpha);
}
