shader_type canvas_item;

uniform sampler2D noise : repeat_enable;
uniform float blood_coef : hint_range(0.0, 1.0) = 0.0;
uniform vec3 blood_color : source_color;
uniform vec4 outline_color = vec4( 0.0, 0.0, 0.0, 1.0 );
uniform float width : hint_range(0, 10) = 1.0;
uniform int pattern : hint_range(0, 2) = 0; // diamond, circle, square
uniform bool inside = false;
uniform bool add_margins = true; // only useful when inside is false
uniform bool use_outline = false;

void vertex() {
	if ( use_outline && add_margins ) {
		VERTEX += ( UV * 2.0 - 1.0 ) * width;
	}
}

bool hasContraryNeighbour(vec2 uv, vec2 texture_pixel_size, sampler2D texture) {
	for (float i = -ceil(width); i <= ceil(width); i++) {
		float x = abs(i) > width ? width * sign(i) : i;
		float offset;

		if (pattern == 0) {
			offset = width - abs(x);
		} else if (pattern == 1) {
			offset = floor(sqrt(pow(width + 0.5, 2) - x * x));
		} else if (pattern == 2) {
			offset = width;
		}

		for (float j = -ceil(offset); j <= ceil(offset); j++) {
			float y = abs(j) > offset ? offset * sign(j) : j;
			vec2 xy = uv + texture_pixel_size * vec2(x, y);

			if ((xy != clamp(xy, vec2(0.0), vec2(1.0)) || texture(texture, xy).a <= 0.0) == inside) {
				return true;
			}
		}
	}

	return false;
}

void fragment() {
	if ( use_outline ) {
		vec2 uv = UV;

		if (add_margins) {
			vec2 texture_pixel_size = vec2(1.0) / (vec2(1.0) / TEXTURE_PIXEL_SIZE + vec2(width * 2.0));

			uv = (uv - texture_pixel_size * width) * TEXTURE_PIXEL_SIZE / texture_pixel_size;

			if (uv != clamp(uv, vec2(0.0), vec2(1.0))) {
				COLOR.a = 0.0;
			} else {
				COLOR = texture(TEXTURE, uv);
			}
		}

		if ((COLOR.a > 0.0) == inside && hasContraryNeighbour(uv, TEXTURE_PIXEL_SIZE, TEXTURE)) {
			COLOR.rgb = inside ? mix(COLOR.rgb, outline_color.rgb, outline_color.a) : outline_color.rgb;
			COLOR.a += (1.0 - COLOR.a) * outline_color.a;
		}
	}

	vec4 curr_texture = texture(TEXTURE, UV);
	vec4 noise_texture = texture(noise, UV);
	float is_blood = step( noise_texture.r, blood_coef);
	noise_texture.r = mix(noise_texture.r, curr_texture.r, is_blood);
	vec4 blood_output = vec4(noise_texture.rgb * blood_color, curr_texture.a);
	COLOR = mix(COLOR, blood_output, is_blood);
}