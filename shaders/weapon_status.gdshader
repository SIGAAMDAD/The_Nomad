shader_type canvas_item;

uniform sampler2D status_icon : filter_nearest;
uniform bool status_active;

void fragment() {
	vec4 color = texture( status_icon, UV );
	if ( !status_active ) {
		color.rgb *= vec3( 0.30 );
	}
	COLOR = color;
}