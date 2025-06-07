shader_type canvas_item;

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	color.b = 0.0;
	color.a = 0.5;
	COLOR = color;
}