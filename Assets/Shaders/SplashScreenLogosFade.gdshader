shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0) = 0.0;

// Shape
uniform vec2 center = vec2(0.5, 0.5);
uniform float aspect = 1.77778; // set from TextureRect size: width / height
uniform float edge_width : hint_range(0.001, 0.3) = 0.075;
uniform float distortion_strength : hint_range(0.0, 0.1) = 0.015;
uniform float noise_scale : hint_range(1.0, 100.0) = 28.0;

// Look
uniform vec4 burn_color : source_color = vec4(1.0, 0.45, 0.08, 1.0);
uniform vec4 ember_color : source_color = vec4(1.0, 0.85, 0.35, 1.0);
uniform vec4 char_color : source_color = vec4(0.06, 0.02, 0.01, 1.0);

// Pixel-art friendliness
uniform bool pixelate_heat = true;
uniform vec2 virtual_resolution = vec2(320.0, 180.0);

// Animation
uniform float time_scale : hint_range(0.0, 5.0) = 1.0;

float hash(vec2 p) {
	p = fract(p * vec2(123.34, 456.21));
	p += dot(p, p + 45.32);
	return fract(p.x * p.y);
}

float noise(vec2 p) {
	vec2 i = floor(p);
	vec2 f = fract(p);

	float a = hash(i);
	float b = hash(i + vec2(1.0, 0.0));
	float c = hash(i + vec2(0.0, 1.0));
	float d = hash(i + vec2(1.0, 1.0));

	vec2 u = f * f * (3.0 - 2.0 * f);

	return mix(a, b, u.x)
		+ (c - a) * u.y * (1.0 - u.x)
		+ (d - b) * u.x * u.y;
}

float fbm(vec2 p) {
	float v = 0.0;
	float a = 0.5;
	for (int i = 0; i < 4; i++) {
		v += noise(p) * a;
		p *= 2.0;
		a *= 0.5;
	}
	return v;
}

void fragment() {
	vec2 uv = UV;

	// Lock distortion to a virtual pixel grid so it behaves better over pixel art
	vec2 heat_uv = uv;
	if (pixelate_heat) {
		heat_uv = (floor(uv * virtual_resolution) + 0.5) / virtual_resolution;
	}

	// Aspect-correct radial distance from center
	vec2 d = uv - center;
	d.x *= aspect;
	float radial = length(d);

	// Animated organic breakup
	float t = TIME * time_scale;
	float n1 = fbm(uv * noise_scale + vec2(0.0, -t * 1.2));
	float n2 = fbm((uv + vec2(17.3, 9.1)) * (noise_scale * 0.55) + vec2(t * 0.35, 0.0));
	float breakup = (n1 * 0.75 + n2 * 0.25) - 0.5;

	// Burn grows from center to edges/corners
	float max_radius = 1.25;
	float burn_radius = progress * max_radius;
	float front = burn_radius + breakup * edge_width * 0.9;

	// > 0 inside already-burned region
	float burn_mask = front - radial;

	// Bands near the front
	float char_band = smoothstep(-edge_width, 0.0, burn_mask)
		- smoothstep(0.0, edge_width * 0.55, burn_mask);

	float ember_band = smoothstep(-edge_width * 0.45, -edge_width * 0.05, burn_mask)
		- smoothstep(-edge_width * 0.05, edge_width * 0.12, burn_mask);

	float rim = smoothstep(-edge_width * 0.08, 0.0, burn_mask)
		- smoothstep(0.0, edge_width * 0.08, burn_mask);

	// Heat distortion only around the advancing front
	float heat_zone = smoothstep(-edge_width * 1.25, edge_width * 0.35, burn_mask)
		* (1.0 - smoothstep(edge_width * 0.15, edge_width * 0.7, burn_mask));

	vec2 flow = vec2(
		fbm(heat_uv * (noise_scale * 0.6) + vec2(t * 0.7, 0.0)) - 0.5,
		fbm(heat_uv * (noise_scale * 0.6) + vec2(0.0, -t * 0.9) + 31.7) - 0.5
	);

	vec2 distorted_uv = uv + flow * distortion_strength * heat_zone;

	// Sample the ViewportTexture assigned to the TextureRect
	vec4 base = texture(TEXTURE, distorted_uv);
	vec3 col = base.rgb;

	// Charred dark zone just behind the fire front
	col = mix(col, char_color.rgb, clamp(char_band * 1.15, 0.0, 1.0));

	// Embers and hot edge
	col += ember_color.rgb * ember_band * 0.55;
	col += burn_color.rgb * rim * 1.2;

	// Burned area becomes transparent, revealing the next scene underneath
	float alpha = 1.0 - smoothstep(0.0, edge_width * 0.85, burn_mask);

	COLOR = vec4(col, base.a * alpha);
}
