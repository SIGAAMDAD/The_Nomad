shader_type canvas_item;

uniform bool alpha_blend = false;
//uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

void fragment() {
	vec4 color = texture( TEXTURE, UV );

	if ( alpha_blend ) {
		color.a *= 0.25;
	}

	COLOR = color;
}