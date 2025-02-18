shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;
uniform bool demon_eye = false;

void fragment() {
	vec4 color = texture( SCREEN_TEXTURE, UV );
	color.rgb = vec3( 0.0, 0.0, 1.0 );
	color.a = 0.0;
	COLOR = color;
}
