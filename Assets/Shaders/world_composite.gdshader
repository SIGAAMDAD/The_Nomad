shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D shadow_texture : repeat_disable, filter_linear;

uniform int debug_view : hint_enum(
	"Composited",
	"Shadow Mask Total",
	"Directional Shadow",
	"Contact AO"
) = 0;

uniform vec3 shadow_color : source_color = vec3(0.0, 0.0, 0.0);

uniform bool enable_fog = true;
uniform bool enable_color_grade = true;
uniform bool enable_vignette = true;
uniform bool enable_color_dither = true;

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

vec3 debug_scalar(float value) {
	return vec3(clamp(value, 0.0, 1.0));
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
	float aspect = pixel_size.y / max(pixel_size.x, 0.000001);
	p.x *= aspect;

	float dist = length(p);
	float vig = smoothstep(vignette_inner, vignette_outer, dist);

	return color * (1.0 - vig * vignette_strength);
}

void fragment() {
	vec4 scene = texture(screen_texture, SCREEN_UV);
	vec4 shadow = texture(shadow_texture, SCREEN_UV);

	float directional = clamp(shadow.r, 0.0, 1.0);
	float contact = clamp(shadow.g, 0.0, 1.0);
	float total = clamp(shadow.b, 0.0, 1.0);

	if (debug_view == 1) {
		COLOR = vec4(debug_scalar(total), 1.0);
		discard;
	}
	if (debug_view == 2) {
		COLOR = vec4(debug_scalar(directional), 1.0);
		discard;
	}
	if (debug_view == 3) {
		COLOR = vec4(debug_scalar(contact), 1.0);
		discard;
	}

	vec3 color = mix(scene.rgb, shadow_color, total);
	color = apply_fog(color, SCREEN_UV);
	color = apply_color_grade(color);
	color = apply_vignette(color, SCREEN_UV, SCREEN_PIXEL_SIZE);

	if (enable_color_dither) {
		color += (hash12(FRAGCOORD.xy) - 0.5) / 255.0;
	}

	COLOR = vec4(color, scene.a);
}
