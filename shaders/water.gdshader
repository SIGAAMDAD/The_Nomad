shader_type canvas_item;

uniform sampler2D noise1 : repeat_enable;
uniform sampler2D noise2 : repeat_enable;
uniform sampler2D screen : hint_screen_texture, filter_linear_mipmap;
uniform vec2 scroll1 = vec2( 0.05, 0.05 );
uniform vec2 scroll2 = vec2( -0.05, -0.05 );
uniform float distortion_strength : hint_range( -1, 1 ) = 0.2;
uniform vec4 tone_color : source_color;
uniform vec4 top_color : source_color;
uniform float light_start : hint_range( 0.0, 1.0 ) = 0.275;
uniform float light_end : hint_range( 0.0, 1.0 ) = 0.4;

void fragment() {
	float depth = texture( noise1, UV + scroll1 * TIME ).r
		* texture( noise2, UV + scroll2 * TIME ).r;
	vec4 screen_col = texture( screen,
		SCREEN_UV + distortion_strength * vec2( depth ) );
	vec4 top_light = smoothstep( light_start, light_end, depth ) * top_color;
	COLOR = screen_col * tone_color + top_light;
//	COLOR = ( texture( screen, SCREEN_UV ) + screen_col ) * tone_color + top_light;
	COLOR.a = 1.0;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
