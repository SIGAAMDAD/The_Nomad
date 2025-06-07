shader_type canvas_item;

uniform sampler2D noise : repeat_enable;
uniform float blood_coef : hint_range(0.0, 1.0) = 0.0;
uniform vec3 color : source_color;

void fragment() {
	vec4 curr_texture = texture(TEXTURE, UV);
	vec4 noise_texture = texture(noise, UV);

	float is_blood = step( noise_texture.r, blood_coef);
	noise_texture.r = mix(noise_texture.r, curr_texture.r, is_blood);
	vec4 blood_output = vec4(noise_texture.rgb * color, curr_texture.a);
	COLOR = mix(curr_texture, blood_output, is_blood);
}