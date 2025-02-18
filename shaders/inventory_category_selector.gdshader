shader_type canvas_item;

uniform bool is_selected = false;
uniform sampler2D icon : filter_nearest;

void fragment() {
	vec4 color = texture( icon, UV );
	if ( !is_selected ) {
		color.rgb *= vec3( 0.30 );
	}
	COLOR = color;
}