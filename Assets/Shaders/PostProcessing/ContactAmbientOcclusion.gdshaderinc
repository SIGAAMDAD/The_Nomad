#ifndef CONTACT_AMBIENT_OCCLUSION_GDSHADERINC
#define CONTACT_AMBIENT_OCCLUSION_GDSHADERINC

#if FX_CONTACT_AO

uniform float contact_ao_radius_px : hint_range(0.0, 64.0) = 10.0;
uniform float contact_ao_strength : hint_range(0.0, 2.0) = 0.55;
uniform float contact_ao_max : hint_range(0.0, 1.0) = 0.35;

// Prevent AO from painting over the caster itself.
uniform bool contact_ao_skip_casters = true;

float sample_contact_mask(vec2 uv) {
	if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
		return 0.0;
	}

	// G = foot/contact mask from height viewport.
	return texture(height_texture, uv).g;
}

float contact_ao(vec2 uv, vec2 pixel_size) {
	vec4 here = texture(height_texture, uv);

	if (contact_ao_skip_casters && here.a > 0.01) {
		return 0.0;
	}

	vec2 r = pixel_size * contact_ao_radius_px;

	float occ = 0.0;
	float taps = 0.0;

#if FX_CONTACT_AO_TAPS >= 4
	occ += sample_contact_mask(uv + vec2( 1.0,  0.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2(-1.0,  0.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2( 0.0,  1.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2( 0.0, -1.0) * r); taps += 1.0;
#endif

#if FX_CONTACT_AO_TAPS >= 8
	occ += sample_contact_mask(uv + vec2( 0.70710678,  0.70710678) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2(-0.70710678,  0.70710678) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2( 0.70710678, -0.70710678) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2(-0.70710678, -0.70710678) * r); taps += 1.0;
#endif

#if FX_CONTACT_AO_TAPS >= 12
	occ += sample_contact_mask(uv + vec2( 1.5,  0.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2(-1.5,  0.0) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2( 0.0,  1.5) * r); taps += 1.0;
	occ += sample_contact_mask(uv + vec2( 0.0, -1.5) * r); taps += 1.0;
#endif

	float ao = occ / max(taps, 1.0);

	// Softer falloff. Keeps contact AO from becoming blocky.
	ao = smoothstep(0.0, 1.0, ao);
	ao *= contact_ao_strength;

	return clamp(ao, 0.0, contact_ao_max);
}

#else

float contact_ao(vec2 uv, vec2 pixel_size) {
	return 0.0;
}

#endif

#endif
