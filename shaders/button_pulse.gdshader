shader_type canvas_item;

uniform vec4 base_color : source_color = vec4( 0.22, 0.22, 0.22, 1.0 );
uniform vec4 glow_color : source_color = vec4( 1.0, 0.68, 0.2, 1.0 );
uniform float pulse_speed : hint_range( 0.5, 4.0 ) = 4.0;
uniform float glow_intensity : hint_range( 0.0, 2.0 ) = 1.5;
uniform float glow_size : hint_range( 0.0, 2.0 ) = 2.0;
uniform bool focused = false;

void fragment() {
	vec4 tex = texture( TEXTURE, UV );
	if ( !focused ) {
		COLOR = tex;
	} else {
		vec3 col = tex.rgb * base_color.rgb;
		
		vec2 center = vec2( 0.5 );
		float dist = distance( UV, center ) * 2.0;
		float glow = 1.0 - smoothstep( 0.0, glow_size, dist );
		
		float pulse = sin( TIME * pulse_speed ) * 0.5 + 0.5;
		float animated_glow = glow * pulse * glow_intensity;
		vec3 final_color = col + glow_color.rgb * animated_glow;
		
		COLOR = vec4( final_color, tex.a );
	}
}