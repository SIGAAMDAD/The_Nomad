shader_type canvas_item;

uniform float sanity = 1.0;
uniform int biome = 0;

uniform sampler2D screen_texture : hint_screen_texture;

uniform float distortion_intensity = 0.02;
uniform sampler2D noise_texture;

uniform vec4 swamp_color_shift = vec4( 0.4, 1.0, 0.3, 1.0 );
uniform float heat_wave_speed = 0.5;
uniform float frost_refraction = 0.1;

void fragment() {
	vec2 uv = UV;
	float low_sanity = 1.0 - sanity;
	vec4 color = texture( screen_texture, uv );

	float global_distortion = low_sanity * distortion_intensity;
	vec2 distortion = vec2(
		sin( TIME * 2.0 + uv.y * 20.0 ) * global_distortion,
		cos( TIME * 1.5 + uv.x * 15.0 ) * global_distortion
	);

	// biome-specific distortion
	switch ( biome ) {
	case 0: // bile swamps
		COLOR = mix(
			texture( screen_texture, uv * distortion ),
			swamp_color_shift,
			low_sanity * 0.3
		);
		break;
	case 1: // safe sands
		vec2 sand_uv = uv + vec2( TIME * 0.1, TIME * 0.07 );
		float sand_noise = texture( noise_texture, sand_uv * 5.0 ).r;
		vec2 sand_distort = distortion + ( sand_noise - 0.5 ) * low_sanity * 0.01;
		COLOR = texture( screen_texture, uv + sand_distort );
		break;
	case 2: // sea of sands
		float heat_wave = sin( uv.y * 20.0 + TIME * heat_wave_speed ) * low_sanity * 0.03;
		vec2 heat_uv = uv + vec2( heat_wave, 0.0 );
		COLOR = texture( screen_texture, heat_uv );
		break;
	case 3: // salt wastes
		float rock_noise = texture( noise_texture, uv * 2.0 ).r;
		vec2 rock_shift = distortion + vec2(
			sin( TIME + uv.y * 10.0 ) * rock_noise * low_sanity * 0.02,
			0.0
		);
		COLOR = texture( screen_texture, uv + rock_shift );
		break;
	case 4: // ashen plains
		vec2 offset = low_sanity * vec2( 0.005, -0.005 );
		vec4 r = texture( screen_texture, uv + offset );
		vec4 g = texture( screen_texture, uv );
		vec4 b = texture( screen_texture, uv - offset );
		COLOR = vec4( r.r, g.g, b.b, color.a );

		float glow = sin( TIME * 2.0 + uv.x * 5.0 ) * low_sanity * 0.3;
		COLOR.r += glow * 0.5;
		break;
	case 5: // dunes of desolation
		vec2 wind_uv = uv + vec2( TIME * 0.05, 0.0 );
		float wind_noise = texture( noise_texture, wind_uv * 3.0 ).r;
		vec2 wind_distort = distortion * ( 1.0 + wind_noise * low_sanity * 0.5 );
		COLOR = texture( screen_texture, uv + wind_distort );
		break;
	case 6: // frozen north
		vec2 frost_uv = uv + ( texture( noise_texture, uv * 4.0 ).rg - 0.5 ) * low_sanity * frost_refraction;
		vec4 frost_color = texture( screen_texture, frost_uv );

		float salt_noise = texture( noise_texture, uv * 2.0 ).r;
		vec2 salt_shift = vec2( sin( TIME * 0.7 ) * salt_noise * low_sanity * 0.01, 0.0 );
		COLOR = texture( screen_texture, frost_uv + salt_shift );

		COLOR.b = mix( COLOR.b, 1.0, low_sanity * 0.1 );
		break;
	default:
		COLOR = color;
		break;
	};
	float luminance = dot( COLOR.rgb, vec3( 0.2126, 0.7152, 0.0722 ) );
	COLOR.rgb = mix( COLOR.rgb, vec3( luminance ), low_sanity * 0.3 );
}