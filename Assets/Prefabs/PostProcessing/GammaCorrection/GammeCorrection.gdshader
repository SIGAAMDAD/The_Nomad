shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_nearest, repeat_enable;

global uniform float brightness_amount = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;
uniform float exposure = 1.0;
uniform int tonemap_mode = 0;
uniform float gamma = 2.2;

vec3 tonemap_aces(vec3 x) {
	float a = 2.51;
    float b = 0.03;
    float c = 2.43;
    float d = 0.59;
    float e = 0.14;
    return clamp( ( x * ( a * x + b ) ) / ( x * ( c * x + d) + e ), 0.0, 1.0 );
}

void fragment() {
	vec3 color = texture( SCREEN_TEXTURE, SCREEN_UV ).rgb;
	
	color *= brightness_amount;
	color = ( color - 0.5 ) * contrast + 0.5;
	
	//float luma = dot( color, vec3( 0.2126, 0.7152, 0.0722 ) );
	//color = mix( vec3( luma ), color, saturation );
	
	/*
	color *= exposure;
	if ( tonemap_mode == 1 ) {
		color = color / ( 1.0 + color );
	} else if ( tonemap_mode == 2 ) {
		color = tonemap_aces( color );
	}
	*/
	
	color = pow( color, vec3( 1.0 / gamma ) );
	COLOR.rgb = color;
}