shader_type spatial;

uniform sampler2D image : filter_nearest;

void fragment() {
	vec4 color = texture( image, UV );
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
