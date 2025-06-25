shader_type canvas_item;

uniform float speed : hint_range( 0.0, 1.0 );

void fragment() {
	vec2 uv = vec2( UV.x, UV.y - speed * TIME );
	vec4 color = texture( TEXTURE, uv );
	if ( color.a > 0.0 ) {
		color.a *= 0.15;
	}
	COLOR = color;
}