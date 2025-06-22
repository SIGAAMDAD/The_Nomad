shader_type canvas_item;

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	if ( color.a > 0.0 ) {
		color.g = mix( color.g, 1.0, 0.8 );
	}
	COLOR = color;
}