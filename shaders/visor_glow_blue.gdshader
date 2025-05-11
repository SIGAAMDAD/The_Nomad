shader_type canvas_item;

uniform vec3 glow_color : source_color;

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	if ( color.b > 0.0 ) {
		color.b += glow_color.b * 10.0;
	}
	COLOR = color;
}