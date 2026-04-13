shader_type canvas_item;

uniform sampler2D noise_tex : repeat_enable;

uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform float scale = 18.0;
uniform float edge_softness : hint_range(0.001, 0.25) = 0.06;
uniform float drift_strength : hint_range(0.0, 0.25) = 0.08;
uniform float ember_strength : hint_range(0.0, 2.0) = 0.5;
uniform float dust_opacity : hint_range(0.0, 2.0) = 1.0;

uniform vec2 wind_dir = vec2(1.0, -0.25);

uniform vec4 dust_color : source_color = vec4(0.82, 0.68, 0.45, 1.0);
uniform vec4 ember_color : source_color = vec4(1.0, 0.42, 0.1, 1.0);

void fragment() {
    vec4 base = texture(TEXTURE, UV);

    // Preserve transparent background around the logo.
    if (base.a < 0.01) {
        discard;
    }

    vec2 dir = normalize(wind_dir);

    // Multi-layered noise for a more natural breakup.
    float n1 = texture(noise_tex, UV * scale).r;
    float n2 = texture(noise_tex, UV * scale * 1.87 + vec2(TIME * 0.07, -TIME * 0.05)).g;
    float n3 = texture(noise_tex, UV * scale * 0.53 - vec2(TIME * 0.03, TIME * 0.02)).b;
    float noise_val = (n1 * 0.5) + (n2 * 0.35) + (n3 * 0.15);

    // Directional wipe: dissolve starts from the right side.
    float directional = 1.0 - UV.x;

    // Mix directional wipe with noise so it feels wind-eroded.
    float dissolve_mask = directional * 0.65 + noise_val * 0.35;

    float threshold = progress;
    float survive = 1.0 - smoothstep(threshold - edge_softness, threshold + edge_softness, dissolve_mask);

    // Edge band where dust and embers appear.
    float edge = smoothstep(threshold - edge_softness * 1.5, threshold, dissolve_mask)
               - smoothstep(threshold, threshold + edge_softness * 1.5, dissolve_mask);

    // Drift the particles in the wind direction.
    vec2 drift_uv = UV - dir * progress * drift_strength * (0.5 + noise_val * 0.5);
    vec4 drift_sample = texture(TEXTURE, drift_uv);

    // Fine sparkle breakup.
    float sparkle_noise = texture(noise_tex, UV * scale * 2.5 + vec2(TIME * 0.2, TIME * 0.1)).r;
    float ember = smoothstep(0.82, 1.0, sparkle_noise) * edge * ember_strength;

    vec3 dust_rgb = mix(dust_color.rgb, ember_color.rgb, ember * 0.75);
    float dust_alpha = drift_sample.a * edge * dust_opacity * (0.65 + sparkle_noise * 0.35);

    vec4 col = vec4(base.rgb, base.a * survive);

    // Dust replacement at the dissolving edge.
    col.rgb = mix(col.rgb, dust_rgb, dust_alpha);
    col.a = max(col.a, dust_alpha);

    // Slight ember glow.
    col.rgb += ember_color.rgb * ember * 0.6;

    // Make remaining particles taper out near the end.
    float tail = 1.0 - smoothstep(0.88, 1.0, progress);
    col.a *= max(survive, tail * edge + survive);

    COLOR = col;
}