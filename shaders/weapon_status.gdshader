shader_type canvas_item;

uniform sampler2D status_icon : filter_nearest;

void fragment() {
	vec4 color = texture( status_icon, UV );
	if ( color.rgb == vec3( 0.0, 0.0, 0.0 ) ) {
		discard;
	}
	COLOR = color;
}