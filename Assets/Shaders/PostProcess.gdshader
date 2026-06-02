shader_type canvas_item;
render_mode unshaded;

// ------------------------------------------------------------
// Compile-time feature profile
// Make quality variants by changing only these defines.
// ------------------------------------------------------------

#ifndef FX_HEIGHT_SHADOWS
#define FX_HEIGHT_SHADOWS 0
#endif

#ifndef FX_SDF_SHADOWS
#define FX_SDF_SHADOWS 0
#endif

#ifndef FX_CONTACT_AO
#define FX_CONTACT_AO 0
#endif

#ifndef FX_CONTACT_AO_TAPS
#define FX_CONTACT_AO_TAPS 8
#endif

#ifndef FX_FOG
#define FX_FOG 1
#endif

#ifndef FX_COLOR_GRADE
#define FX_COLOR_GRADE 1
#endif

#ifndef FX_VIGNETTE
#define FX_VIGNETTE 1
#endif

#ifndef FX_DITHER
#define FX_DITHER 1
#endif

#ifndef FX_SHADOW_STEPS
#define FX_SHADOW_STEPS 32
#endif

// ------------------------------------------------------------
// Shared inputs
// ------------------------------------------------------------

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// Height buffer rendered by HeightViewport.
// R = normalized height.
// G = contact / foot mask.
// A = caster silhouette mask.
uniform sampler2D height_texture : repeat_disable, filter_nearest;

// Shared shadow color controls.
uniform vec3 shadow_color : source_color = vec3(0.0, 0.0, 0.0);
uniform float shadow_opacity : hint_range(0.0, 1.0) = 0.38;

// ------------------------------------------------------------
// Shared utility
// ------------------------------------------------------------

float hash12(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

// ------------------------------------------------------------
// Feature includes
// ------------------------------------------------------------

#include "res://Assets/Shaders/PostProcessing/SDFShadows.gdshaderinc"
#include "res://Assets/Shaders/PostProcessing/ContactAmbientOcclusion.gdshaderinc"
#include "res://Assets/Shaders/PostProcessing/Vignette.gdshaderinc"

// ------------------------------------------------------------
// Fog / haze
// ------------------------------------------------------------

#if FX_FOG
uniform vec3 fog_color : source_color = vec3(0.62, 0.68, 0.78);
uniform float fog_strength : hint_range(0.0, 1.0) = 0.12;
uniform float fog_start_y : hint_range(0.0, 1.0) = 0.15;
uniform float fog_end_y : hint_range(0.0, 1.0) = 1.0;

vec3 apply_fog(vec3 color, vec2 uv) {
	float fog = smoothstep(fog_start_y, fog_end_y, uv.y) * fog_strength;
	return mix(color, fog_color, clamp(fog, 0.0, 1.0));
}
#endif

// ------------------------------------------------------------
// Color grade
// ------------------------------------------------------------

#if FX_COLOR_GRADE
uniform float exposure : hint_range(0.0, 4.0) = 1.0;
uniform float contrast : hint_range(0.0, 3.0) = 1.06;
uniform float saturation : hint_range(0.0, 3.0) = 1.04;
uniform float gamma_value : hint_range(0.2, 3.0) = 1.0;
uniform vec3 grade_tint : source_color = vec3(1.0, 1.0, 1.0);

vec3 apply_color_grade(vec3 color) {
	color *= exposure;

	color = (color - vec3(0.5)) * contrast + vec3(0.5);

	float luma = dot(color, vec3(0.299, 0.587, 0.114));
	color = mix(vec3(luma), color, saturation);

	color *= grade_tint;

	color = pow(max(color, vec3(0.0)), vec3(1.0 / max(gamma_value, 0.001)));

	return color;
}
#endif

// ------------------------------------------------------------
// Main
// ------------------------------------------------------------

void fragment() {
	vec4 scene = texture(screen_texture, SCREEN_UV);
	vec3 color = scene.rgb;

#if FX_FOG
	color = apply_fog(color, SCREEN_UV);
#endif

#if FX_COLOR_GRADE
	color = apply_color_grade(color);
#endif

#if FX_VIGNETTE
	color = apply_vignette(color, SCREEN_UV, SCREEN_PIXEL_SIZE);
#endif

#if FX_DITHER
	color += (hash12(FRAGCOORD.xy) - 0.5) / 255.0;
#endif
	
	COLOR = vec4(color, scene.a);
}
