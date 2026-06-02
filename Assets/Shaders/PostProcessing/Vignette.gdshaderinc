#ifdef FX_VIGNETTE
// ------------------------------------------------------------
// Vignette
// ------------------------------------------------------------

uniform float vignette_strength : hint_range(0.0, 1.0) = 0.18;
uniform float vignette_inner : hint_range(0.0, 2.0) = 0.45;
uniform float vignette_outer : hint_range(0.0, 2.0) = 1.15;

vec3 apply_vignette(vec3 color, vec2 uv, vec2 pixelSize) {
	if (vignette_strength <= 0.0) {
		return color;
	}

	vec2 p = uv * 2.0 - 1.0;

	// Aspect correction from SCREEN_PIXEL_SIZE.
	float aspect = pixelSize.y / max(pixelSize.x, 0.000001);
	p.x *= aspect;

	float dist = length(p);
	float vig = smoothstep(vignette_inner, vignette_outer, dist);

	return color * (1.0 - vig * vignette_strength);
}
#endif
