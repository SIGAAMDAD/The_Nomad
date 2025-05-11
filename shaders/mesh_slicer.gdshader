// from https://godotshaders.com/shader/slice-2d-pixel-perfect-slice-shader/

shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

// Center of the slice, normalized [0,1]
uniform vec2 slice_center = vec2(0.5, 0.5);

// Angle of the slice in radians
uniform float slice_angle = 0.0;

// Distance to move the slices apart, normalized [0,1]
uniform float slice_distance = 0.0;

// Pixel size for pixel-perfect rendering
uniform float pixel_size = 1.0;

// Expansion factor for the sprite bounds
uniform float expansion_factor = 0.5;

// Border color for the expanded area
uniform vec4 border_color = vec4(0.0, 0.0, 0.0, 0.0);

// Convert UV to pixel-perfect UV
vec2 pixel_uv(vec2 uv, float pixel_sizee) {
	return floor(uv * pixel_sizee) / pixel_sizee;
}

// Rotate a point around another point
vec2 rotate(vec2 point, vec2 center, float angle) {
	vec2 translated_point = point - center;
	float sin_angle = sin(angle);
	float cos_angle = cos(angle);
	vec2 rotated_point = vec2(
		translated_point.x * cos_angle - translated_point.y * sin_angle,
		translated_point.x * sin_angle + translated_point.y * cos_angle
	);
	return rotated_point + center;
}

void vertex() {
	// Expand the vertex positions and UVs
	VERTEX *= (1.0 + expansion_factor * 2.0);
	UV = (UV - 0.5) * (1.0 + expansion_factor * 2.0) + 0.5;
}

void fragment() {
	vec2 uv = UV;

	// Adjust UV to handle pixel-perfect rendering
	uv = pixel_uv(uv, pixel_size);

	// Calculate the vector from the slice center to the current UV
	vec2 offset = uv - slice_center;

	// Rotate the offset vector by the negative slice angle
	vec2 rotated_offset = rotate(offset, vec2(0.0), -slice_angle);

	// Determine which side of the slice the current UV is on
	bool is_top = rotated_offset.y > 0.0;

	// Move the UV based on the slice distance
	vec2 movement = vec2(
		slice_distance * cos(slice_angle),
		slice_distance * sin(slice_angle)
	);

	if (is_top) {
		uv += movement;
	} else {
		uv -= movement;
	}

	// Check if UV is outside the [0, 1] range and set the border color if it is
	if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
		COLOR = border_color;
	} else {
		// Sample the texture with the modified UV
		COLOR = texture(TEXTURE, uv);
	}
}
