shader_type canvas_item;

uniform vec3 glow_color : source_color;

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	if ( color.r > 0.0 ) {
		color.r += glow_color.r * 10.0;
	}
	COLOR = color;
}