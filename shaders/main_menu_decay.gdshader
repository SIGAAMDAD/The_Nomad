shader_type canvas_item;

uniform bool decay_active = false;
uniform float decay_timer = 0.0;
uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

void fragment() {
	vec4 base_color = texture( screen_texture, SCREEN_UV );
	float decay_factor = 0.0;
	
	if ( decay_active ) {
		decay_factor = clamp( decay_timer / 10.0, 0.0, 0.15 );
		if ( fract( sin( TIME * 10.0 ) ) > 0.995 ) {
			base_color.rgb *= 4.0;
		}
	}
	
	base_color.rgb *= ( 1.0 - decay_factor );
	COLOR = base_color;
}