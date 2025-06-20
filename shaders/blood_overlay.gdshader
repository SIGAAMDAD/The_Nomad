shader_type canvas_item;

// Blood splotch settings
uniform sampler2D noise_tex;
uniform float blood_speed = 0.5;
uniform float blood_intensity = 0.8;
uniform vec4 blood_color : source_color = vec4(0.5, 0, 0, 1);
uniform float blood_scale = 3.0;

// Vignette settings
uniform float vignette_intensity = 0.7;
uniform vec4 vignette_color : source_color = vec4(0.8, 0, 0, 1);
uniform float vignette_size = 0.7;

void fragment() {
    vec2 uv = UV;

    // Blood splotches (flowing downward)
    vec2 blood_uv = uv * blood_scale;
    blood_uv.y += TIME * blood_speed;
    float noise = texture(noise_tex, blood_uv).r;
    float blood = smoothstep(0.4, 0.6, noise) * blood_intensity;

    // Vignette (red border)
    float dist = distance(uv, vec2(0.5));
    float vignette = smoothstep(vignette_size, 1.0, dist) * vignette_intensity;

    // Combine effects
    vec4 final_color = blood_color * blood;
    final_color += vignette_color * vignette * (1.0 - blood);

    COLOR = final_color;
}