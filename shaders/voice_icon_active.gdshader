shader_type canvas_item;

uniform bool active = false;

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	if ( !active && color.a > 0.0 ) {
		color.a = 0.5;
	}
	COLOR = color;
}