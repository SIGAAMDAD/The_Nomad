shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform sampler2D noise_tex : repeat_enable;

uniform vec2 wind_dir = vec2(1.0, -0.15);
uniform float dissolve_scale : hint_range(1.0, 64.0) = 14.0;
uniform float edge_width : hint_range(0.001, 0.25) = 0.08;
uniform float grain_strength : hint_range(0.0, 2.0) = 1.0;
uniform float drift_distance : hint_range(0.0, 0.5) = 0.12;
uniform float alpha_clip : hint_range(0.0, 1.0) = 0.02;

uniform vec4 sand_color : source_color = vec4(0.84, 0.73, 0.52, 1.0);
uniform vec4 ember_color : source_color = vec4(1.0, 0.45, 0.12, 1.0);
uniform float ember_amount : hint_range(0.0, 1.0) = 0.18;
uniform float glow_strength : hint_range(0.0, 4.0) = 1.25;

void fragment() {
    vec4 base = texture(TEXTURE, UV);

    if (base.a <= alpha_clip) {
        discard;
    }

    vec2 dir = normalize(wind_dir);
    vec2 noise_uv = UV * dissolve_scale;

    float n1 = texture(noise_tex, noise_uv).r;
    float n2 = texture(noise_tex, noise_uv * 1.73 + vec2(TIME * 0.12, -TIME * 0.08)).g;
    float n = mix(n1, n2, 0.45) * grain_strength;

    float threshold = progress;
    float edge0 = threshold - edge_width;
    float edge1 = threshold + edge_width;

    // Main dissolve mask
    float keep_mask = 1.0 - smoothstep(edge0, edge1, n);

    // Edge band where sand/embers appear
    float edge_mask = smoothstep(edge0, threshold, n) - smoothstep(threshold, edge1, n);

    // Drift the edge fragments in the wind direction as progress increases
    vec2 drift_uv = UV - dir * progress * drift_distance * (0.35 + n * 0.65);
    vec4 drift_sample = texture(TEXTURE, drift_uv);

    float grain = texture(noise_tex, noise_uv * 2.4 + vec2(TIME * 0.35, TIME * 0.1)).b;
    float sparkle = smoothstep(0.82, 1.0, grain) * edge_mask;

    vec3 dust_col = mix(sand_color.rgb, ember_color.rgb, ember_amount * sparkle);

    // Surviving base image
    vec4 final_col = vec4(base.rgb, base.a * keep_mask);

    // Sand edge overlay
    float dust_alpha = edge_mask * drift_sample.a * (0.55 + grain * 0.45);
    final_col.rgb = mix(final_col.rgb, dust_col, dust_alpha);
    final_col.a = max(final_col.a, dust_alpha * 0.9);

    // Small emissive-looking ember edge
    final_col.rgb += ember_color.rgb * sparkle * glow_strength * ember_amount * edge_mask;

    // Fade remaining particles near the end so it fully disappears
    float tail_fade = 1.0 - smoothstep(0.88, 1.0, progress);
    final_col.a *= max(tail_fade, keep_mask);

    COLOR = final_col;
}