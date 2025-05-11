shader_type canvas_item;

// Uniform for the cooldown progress, ranges from 0.0 (full cooldown) to 1.0 (no cooldown)
uniform float cooldown_progress : hint_range(0.0, 1.0);

// Uniform to toggle between disappearing or transparent unseen area
uniform bool use_transparency;// : hint_tooltip("Toggle to make unseen area transparent");

// Uniform to control the alpha value of the transparent unseen area
uniform float transparency_alpha : hint_range(0.0, 1.0);

// Function to check if a point is inside the cooldown arc
bool in_cooldown(vec2 uv, float progress) {
    // Convert UV coordinates to centered coordinates (-0.5 to 0.5)
    vec2 centered_uv = uv - vec2(0.5);

    // Calculate the angle of the UV coordinate
    float angle = atan(centered_uv.y, centered_uv.x) / (2.0 * PI) + 0.5;

    // Check if the point is within the cooldown arc
    return (angle < progress);
}

void fragment() {
    // Sample the texture at the given UV coordinates
    vec4 tex_color = texture(TEXTURE, UV);

    // Check if the current fragment is within the cooldown arc
	// If within the cooldown, set the color to the texture color
	// otherwise, check the transparency toggle
	COLOR = in_cooldown( UV, cooldown_progress ) ? tex_color :
		use_transparency ? vec4( tex_color.rgb, transparency_alpha ) : vec4( 0.0, 0.0, 0.0, 0.0 );
}
