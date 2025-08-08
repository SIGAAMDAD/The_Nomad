shader_type canvas_item;

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	if ( color.a > 0.0 ) {
		color.rgb = vec3( 1.0, 1.0, 1.0 );
	}
	COLOR = color;
}