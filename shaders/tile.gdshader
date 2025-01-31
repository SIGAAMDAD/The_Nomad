shader_type canvas_item;

uniform bool PlayerBehindTile;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 color = texture( TEXTURE, UV );
	if ( PlayerBehindTile ) {
		color.a = 0.10f;
	}
	COLOR = color;
	// Called for every pixel the material is visible on.
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
