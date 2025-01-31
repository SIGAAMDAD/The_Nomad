shader_type canvas_item;

uniform sampler2D u_ScreenTexture : hint_screen_texture, filter_linear_mipmap;
uniform float amount : hint_range( 0.0, 5.0 );

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 color = textureLod( u_ScreenTexture, SCREEN_UV, amount );
	COLOR = color;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
