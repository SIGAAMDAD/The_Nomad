// By Amy Gilhespy.
// I hereby dedicate this shader to the public domain.
// https://creativecommons.org/publicdomain/zero/1.0/

shader_type canvas_item;

uniform float health : hint_range(0.0, 1.0) = 0.5;
uniform float trail : hint_range(0.0, 1.0) = 0.6;

uniform float width = 800.0;
uniform float height = 20.0;

uniform sampler2D health_gradient;
uniform vec4 damaged_health_color: source_color = vec4(1.02227, 0.36054, 0.36266, 1.0);
uniform vec4 healed_health_color: source_color = vec4(0.87182, 0.98235, 0.90501, 1.0);
uniform vec4 edge_color: source_color = vec4(vec3(4.0), 1.0);
uniform float edge_pixels = 4.0;
uniform vec4 background_color: source_color = vec4(vec3(0.125), 1.0);
uniform vec4 border_color: source_color = vec4(vec3(0.125), 0.5);
uniform float border_pixels = 1.0;

uniform bool round_corners = false;
uniform float skew_pixels = 10.0;

uniform vec4 top_quarter_modulate: source_color = vec4(vec3(2.0), 1.0);
uniform vec4 top_half_modulate: source_color = vec4(vec3(1.0), 1.0);
uniform vec4 bottom_half_modulate: source_color = vec4(vec3(1.0), 1.0);
uniform vec4 bottom_quarter_modulate: source_color = vec4(vec3(0.375), 1.0);

vec4 get_health_color() {
	return texture(health_gradient, vec2(health, 0.0));
}

vec4 get_border_color(vec2 uv, float hx) {
	if (uv.x < -hx || uv.x > 1.0 + hx) {
		return vec4(vec3(1.0), 0.0);
	}
	return border_color;
}

void fragment() {
	float aspect = width / height;
	float horizontal_border_u = border_pixels / width;
	float vertical_border_v = border_pixels / height;
	float edge_width = edge_pixels / width;
	vec2 uv0 = UV;
	//vec2 px0 = vec2((uv0.x - 0.5) * width, (uv0.y - 0.5) * height);
	float non_border_width = width - 2.0 * border_pixels - abs(skew_pixels);
	float non_border_height = height - 2.0 * border_pixels;
	float skew = skew_pixels >= 0.0 ? 1.0 - uv0.y : uv0.y;
	vec2 uv = vec2(
		(uv0.x - horizontal_border_u - skew * abs(skew_pixels) / non_border_width) / (1.0 - 2.0 * horizontal_border_u - abs(skew_pixels) / non_border_width),
		(uv0.y - vertical_border_v) / (1.0 - 2.0 * vertical_border_v));
	vec4 color = vec4(vec3(1.0), 0.0);
	bool is_background = false;
	bool is_edge = false;
	if (trail <= health) {
		if (uv.x <= trail) {
			color = get_health_color();
			if (uv.x > health - edge_width) {
				is_edge = true;
			}
		} else if (uv.x <= health) {
			color = healed_health_color;
			if (uv.x > health - edge_width) {
				is_edge = true;
			}
		} else {
			is_background = true;
			color = background_color;
		}
	} else {
		if (uv.x <= health) {
			color = get_health_color();
			if (uv.x > health - edge_width) {
				is_edge = true;
			}
		} else if (uv.x <= trail) {
			color = damaged_health_color;
		} else {
			is_background = true;
			color = background_color;
		}
	}
	if (!is_background) {
		if (uv.y >= 0.5) {
			color *= bottom_half_modulate;
			if (uv.y >= 0.75) {
				color *= bottom_quarter_modulate;
			}
		} else {
			color *= top_half_modulate;
			if (uv.y < 0.25) {
				color *= top_quarter_modulate;
			}
		}
	}
	if (is_edge) {
		color = edge_color;
		if (round_corners) {
			vec2 px = vec2((uv.x - 0.5) * non_border_width, (uv.y - 0.5) * non_border_height);
			float r = 0.5 * non_border_height;
			float y = px.y;
			float x = sqrt(max(0.0, r * r - y * y));
			if (px.x + non_border_width * 0.5 < r - x) {
				color = get_border_color(uv, horizontal_border_u);
			} else if (px.x > non_border_width * 0.5 - (r - x)) {
				color = get_border_color(uv, horizontal_border_u);
			}
		}
	} else if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
		color = get_border_color(uv, horizontal_border_u);
	} else if (round_corners) {
		vec2 px = vec2((uv.x - 0.5) * non_border_width, (uv.y - 0.5) * non_border_height);
		float r = 0.5 * non_border_height;
		float y = px.y;
		float x = sqrt(max(0.0, r * r - y * y));
		if (px.x + non_border_width * 0.5 < r - x) {
			color = get_border_color(uv, horizontal_border_u);
		} else if (px.x > non_border_width * 0.5 - (r - x)) {
			color = get_border_color(uv, horizontal_border_u);
		}
	}
	COLOR *= color;
}
