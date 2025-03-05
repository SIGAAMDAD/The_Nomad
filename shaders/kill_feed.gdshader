shader_type canvas_item;

uniform sampler2D icon : filter_nearest;

void fragment() {
	vec4 color = texture( icon, UV );
	color.rgb *= vec3( 1.0 );
	COLOR = color;
}