shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest;
uniform float alpha : hint_range( 0.0, 1.0 );

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	color.a = alpha;
	COLOR = color;
}