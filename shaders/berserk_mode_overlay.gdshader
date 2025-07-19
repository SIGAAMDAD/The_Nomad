shader_type canvas_item;

uniform float vignette_intensity : hint_range( 0, 1 ) = 0.3;
uniform float desaturation : hint_range( 0, 1 ) = 0.2;
uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

void fragment() {
	vec3 color = textureLod( screen_texture, SCREEN_UV, 0.0 ).rgb;

	vec2 uv = SCREEN_UV * ( 1.0 - SCREEN_UV.yx );
	float vig = uv.x * uv.y * 15.0;
	vig = pow( vig, vignette_intensity );

	float luminance = dot( color, vec3( 0.299, 0.587, 0.114 ) );
	color = mix( color, vec3( luminance ), desaturation );

	COLOR.rgb = color * vig;
}