shader_type spatial;

uniform float sanity;

void vertex() {
	float insanity = 1.0 - sanity;
	if ( insanity > 0.50 ) {
		VERTEX += NORMAL * sin( TIME * 0.75 ) * insanity * 0.1;
	}
}