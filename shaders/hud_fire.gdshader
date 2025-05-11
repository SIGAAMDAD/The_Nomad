/*
	炎シェーダー by あるる（きのもと 結衣）
	Fire Shader by Yui Kinomoto @arlez80

	MIT License
*/
shader_type canvas_item;
render_mode unshaded;

//Noise and color uniforms
uniform sampler2D	noise_tex			: hint_default_white, repeat_enable;
uniform vec4		root_color			: source_color = vec4(1.0, 0.75, 0.3, 1.0);
uniform vec4		tip_color			: source_color = vec4(1.0, 0.03, 0.001, 1.0);
uniform float		poster_color		: hint_range(0.0, 16.0) = 6.0;
//Fire stuff
uniform float		fire_alpha			: hint_range(0.0, 1.0) = 1.0;
uniform vec2		fire_speed			= vec2(0.015, 0.5);
uniform float		fire_aperture		: hint_range(0.0, 5.0) = 0.22;
//Vignette
uniform float		vignette_radius		: hint_range(0.0, 1.0) = 0.25; //fade start
uniform float		vignette_falloff	: hint_range(0.0, 0.5) = 0.25; //fade width
uniform float		noise_influence		: hint_range(0.0, 1.0) = 1.00; //noise influence

vec2 polar_coordinates(vec2 uv, vec2 center, float zoom, float repeat) {
    vec2 d = uv - center;
    float r     = length(d) * 2.0;
    float theta = atan(d.y, d.x) * (1.0 / (2.0 * 3.1416));
    return mod(vec2(r * zoom, theta * repeat), 1.0);
}

void fragment() {
	vec2  center = vec2(0.5);
	vec2  p = polar_coordinates(UV, center, 1.0, 1.0);

	//fire "movement"
	p.x += TIME * fire_speed.y;
	p.y += sin(TIME) * fire_speed.x; //-> X is rotation!

	//noise texture
	//float n = texture(noise_tex, vec2(p.x , p.y * PI)).r; //alternative to a large noise texture!
	float n = texture(noise_tex, p).r;

	//the fire itslef
	float dist = distance(UV, center);
	float edge = clamp(1.0 - dist, 0.0, 1.0);
	float noise_val = edge * (((edge + fire_aperture) * n - fire_aperture) * 75.0);
	noise_val = clamp(noise_val, 0.0, 1.0);

	//vignette
	float effective_radius = vignette_radius + n * noise_influence * vignette_falloff;
	float mask = smoothstep(effective_radius + vignette_falloff, effective_radius, 1.0-dist);

	//final alpha
	float alpha = noise_val * fire_alpha * mask;

	//color posterization
	vec4 fire_color;
	if (poster_color >= 1.0){
		float quantized = floor(n * poster_color) / poster_color;
		fire_color = mix(tip_color, root_color, quantized);
		alpha = floor(alpha * poster_color) / poster_color;
	}
	else{
		fire_color = mix(tip_color, root_color, n);
	}

	COLOR = vec4(fire_color.rgb, alpha);
}