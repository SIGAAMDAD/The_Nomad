shader_type canvas_item;

uniform sampler2D source_texture : hint_screen_texture;
uniform float render_scale : hint_range( 0.1, 1.0 ) = 0.5;

vec3 fsr_easu( vec2 uv, vec2 source_size, vec2 output_size ) {
	vec2 pixel_pos = uv * source_size - 0.5;
	vec2 f = fract( pixel_pos );
	vec2 pixel_center = floor( pixel_pos ) + 0.5;

	vec3 samples[16];
	for ( int y = 0; y < 4; y++ ) {
		for ( int x = 0; x < 4; x++ ) {
			vec2 sample_uv = ( pixel_center + vec2( float( x ) - 1.0, float( y ) - 1.0 ) ) / source_size;
			samples[ y * 4 + x ] = texture( source_texture, sample_uv ).rgb;
		}
	}

	vec2 w0 = ( 1.0 - f ) * ( 1.0 - f ) * ( 1.0 - f );
	vec2 w1 = f * f * ( 3.0 - 2.0 * f );

	vec3 a = mix( mix( samples[5], samples[6], w1.x ), mix( samples[9], samples[10], w1.x ), w1.y );
	vec3 b = mix( mix( samples[6], samples[7], w1.x ), mix( samples[10], samples[11], w1.x ), w1.y );

	return mix( a, b, w0.x );
}

void fragment() {
	vec2 source_size = 1.0 / TEXTURE_PIXEL_SIZE;
	vec2 output_size = 1.0 / SCREEN_PIXEL_SIZE;

	vec3 color;

	if ( render_scale > 0.75 ) {
		// simple bilinear filter
		color = texture( source_texture, UV ).rgb;
	} else if ( render_scale > 0.5 ) {
		// FSR1
		color = fsr_easu( UV, source_size, output_size );
	} else {
		vec2 texel_size = 1.0 / source_size;
		vec3 center = texture( source_texture, UV ).rgb;
		vec3 sum = vec3( 0.0 );

		for ( int y = -1; y <= 1; y++ ) {
			for ( int x = -1; x <= 1; x++ ) {
				sum += texture( source_texture, UV + vec2( float( x ), float( y ) ) * texel_size ).rgb;
			}
		}

		vec3 blur = sum / 9.0;
		color = center + ( center - blur ) * 0.5;
	}

	COLOR = vec4( color, 1.0 );
}