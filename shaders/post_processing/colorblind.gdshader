shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform int mode = 0;

void fragment() {
	vec4 color = texture( screen_texture, SCREEN_UV );
	switch ( mode ) {
	case 0:
		color = color.gbra;
		break;
	case 1:
		color = color.brga;
		break;
	case 2:
		color = color.bgra;
		break;
	};

	COLOR = vec4( color.rgb, 1.0 );
}