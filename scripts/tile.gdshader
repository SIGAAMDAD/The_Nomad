shader_type spatial;

uniform bool is_player_colliding = true;

void fragment() {
	if ( is_player_colliding ) {
		ALPHA = 0.1;
	}
}