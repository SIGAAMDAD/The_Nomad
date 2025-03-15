shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap_anisotropic, repeat_disable;
uniform float hpass : hint_range(0.0, 1.0, 0.1) = 1.0;
uniform float vpass : hint_range(0.0, 1.0, 0.1) = 1.0;
uniform int radius : hint_range(0, 65, 1) = 65;
uniform vec2 unit = vec2(1., 1.);
render_mode blend_add;

vec4 textureThresholded(sampler2D _texture, vec2 _uv, float _bias) {
	vec4 pixel = textureLod(_texture, _uv, _bias);

	if ( pixel.r <= 1. && pixel.g <= 1. && pixel.b <= 1. ) {
		pixel.rgb = vec3(0.);
	}

	return pixel;
}

void fragment() {
	vec4 pixel = textureThresholded(screen_texture, SCREEN_UV, 0.);

	if (radius != 0) {

		vec4 blurred = vec4(0., 0., 0., 1.);
		float[65] w = {0.0064, 0.0063, 0.0062, 0.0061, 0.006,
			0.0059, 0.0058, 0.0057, 0.0056, 0.0055, 0.0054, 0.0053, 0.0052, 0.0051, 0.005,
			0.0049, 0.0048, 0.0047, 0.0046, 0.0054, 0.0044, 0.0043, 0.0042, 0.0041, 0.004,
			0.0039, 0.0038, 0.0037, 0.0036, 0.0043, 0.0034, 0.0033, 0.0032, 0.0031, 0.003,
			0.0029, 0.0028, 0.0027, 0.0026, 0.0052, 0.0024, 0.0023, 0.0022, 0.0021, 0.002,
			0.0019, 0.0018, 0.0017, 0.0016, 0.0051, 0.0014, 0.0013, 0.0012, 0.0011, 0.001,
			0.0009, 0.0008, 0.0007, 0.0006, 0.0005, 0.0004, 0.0003, 0.0002, 0.0001, 0.
		};

		float px = 1. / float(textureSize(screen_texture, 0).x) * unit.x;
		float py = 1. / float(textureSize(screen_texture, 0).y) * unit.y;

		for(int i = 0; i < radius; i++) {
			float k = float(i + 1);

			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(i) * cos(float(i)), 0) * vec2(px, py) * vec2(hpass, vpass) * vec2(hpass, vpass), k) * w[i];
			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(-i) * cos(float(i)), 0) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];

			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(0, float(i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];
			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(0, float(-i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];

			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(i) * cos(float(i)), float(i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];
			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(-i) * cos(float(i)), float(-i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];

			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(i) * cos(float(i)), float(-i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];
			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(-i) * cos(float(i)), float(i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];

			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(-i) * cos(float(i)), float(i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];
			blurred += textureThresholded(screen_texture, SCREEN_UV + vec2(float(i) * cos(float(i)), float(-i) * sin(float(i))) * vec2(px, py) * vec2(hpass, vpass), k) * w[i];
		}

		blurred /= float(radius) / 6.;
		pixel += blurred;

	} else {
		pixel = vec4(0., 0., 0., 1.);
	}

	COLOR = pixel;
}