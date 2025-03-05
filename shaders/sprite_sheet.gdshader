shader_type canvas_item;

uniform sampler2D sprite_sheet : filter_nearest;
uniform float time_offset = 0.0;
uniform int frame_count = 0;
uniform float time_scale = 1.0;
uniform float animation_duration = 0.0;

void fragment() {
	float time = TIME * time_offset;

	float progress = fract( time / ( animation_duration / max( time_scale, 0.0000001 ) ) );
	int frame = int( floor( progress * float( frame_count ) ) );

	vec2 frame_uv = UV / vec2( float( frame_count ), 1.0 );
	frame_uv.x += float( frame ) / float( frame_count );

	vec4 texture_color = texture( sprite_sheet, frame_uv );
	COLOR = texture_color;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
