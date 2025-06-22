shader_type canvas_item;
render_mode unshaded;

uniform float outline_size : hint_range(0, 5) = 1.0;
uniform vec4 outline_color : source_color = vec4(0.08, 0.05, 0.03, 1.0);
uniform float brush_scale = 8.0;
uniform float interior_ink_variation = 0.3;
uniform sampler2D brush_texture;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

void fragment() {
    // Get screen texture and pixel size
    vec2 screen_uv = SCREEN_UV;
    vec2 pixel_size = SCREEN_PIXEL_SIZE;

    // Sample screen texture
    vec4 col = texture(SCREEN_TEXTURE, screen_uv);

    // Early exit if pixel is fully transparent
    if (col.a < 0.01) {
        COLOR = col;
		discard;
    }

    // Sample brush texture (with random rotation)
    vec2 brush_uv = screen_uv * brush_scale;
    float brush = texture(brush_texture, brush_uv).r;

    // Edge detection with brush variation
    float max_alpha = 0.0;
    float outline_thickness = outline_size * mix(0.8, 1.2, brush);

    // Check surrounding pixels
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            if (x == 0 && y == 0) continue;

            vec2 offset = vec2(float(x), float(y)) * pixel_size * outline_thickness;
            vec4 sample_col = texture(SCREEN_TEXTURE, screen_uv + offset);
            max_alpha = max(max_alpha, sample_col.a);
        }
    }

    // Create outline effect
    float outline_alpha = max_alpha * (1.0 - col.a);
    outline_alpha *= smoothstep(0.3, 0.7, brush); // Organic edge

    // Apply ink variation to interior
    vec3 ink_variation = vec3(1.0 - interior_ink_variation * brush);
    vec3 interior = col.rgb * ink_variation;

    // Combine elements
    vec3 final_color = mix(outline_color.rgb, interior, col.a);
    float final_alpha = max(col.a, outline_alpha * outline_color.a);

    COLOR = vec4(final_color, final_alpha);
}