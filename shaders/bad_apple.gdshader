// Texture pixelate shader written in Godot 3.3.2.
// Made by Hei and distributed under MIT-license.
//
// Hey this is Hei! I'll hope you put this shader to good use. <3
// Example gd-script can be found as a comment at the bottom.

// PREREQUISITES:
// 1. attach to a node with TEXTURE (VideoPlayer used as example).
// 2. cell_texture must be set.
// 3. rect_size must be set by hand or from script (see example).
// 4. blur requires enabling mipmaps (and filtering) (see example).
// NOTES:
// 1. It is possible to further optimize this shader especially
// when using cell_sizes of power 2.
// 2. Odd numbered cell_sizes may or may not cause visual artifacts.
// 3. Blur amount is a bit hacky and great amounts may result in artifacts.

shader_type canvas_item;

uniform vec2 rect_size = vec2(1024, 600);
// Changes the "pixelation". Size limits aren't necessary.
// By default uses square shape but can be be changed to vec2 for rectangles.
uniform float cell_size : hint_range(2, 64) = 16.0;
// Texture of the cell/pixel
uniform sampler2D cell_texture : hint_default_black;
// Reverses value. Mainly when used with cutoff
uniform bool reverse = false;
// Snaps the value to either 0 or 1 based on the cutoff_value
uniform bool cutoff = false;
uniform float cutoff_value : hint_range(0, 1) = 0.5;
// Enabling desaturates the cell_texture and uses the color from TEXTURE.
// Disabling will use the cell_textures color.
uniform bool colored = true;
// Extra brightness. Useful when the albedo texture is  dark.
uniform float brightness : hint_range(0, 2) = 1.0;
// Adds colored corners to the albedo. Not necessary to work
uniform float corner_size : hint_range(0, 1) = 0.2;
uniform vec4 corner_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
// Blur is based on the texture mipmaps (downscaled versions of the texture)
// and is therefore quite fast (and accurate when cell_size is a power of 2.)
uniform float blur_amount : hint_range(0, 1) = 0.5;

void fragment() {
	vec2 cell_scale = cell_size / rect_size;
	vec2 coord = UV / cell_scale;
	vec2 uv = fract(coord);
	// To disable corners alltogether, remove following line and if statement.
	vec2 dis = abs(uv - 0.5);
	if (dis.x + dis.y > 1.0 - corner_size) {
		COLOR = corner_color;
	}
	else {
		// Snaps uv to nearest cells corner and offsets it to cell center
		vec2 source_uv = (floor(coord) + 0.5) * cell_scale;
		float mipmap = ceil(log2(cell_size) * blur_amount);
		vec3 source = textureLod(TEXTURE, source_uv, mipmap).rgb;

		float sat = (source.r +source.g +source.b) / 3.0;
		if (reverse) sat = 1.0 - sat;
		if (cutoff) sat = step(cutoff_value, sat);
		if (colored) {
			// Desaturates albedo and uses source saturation
			vec3 cell = texture(cell_texture, uv).rgb;
			vec3 grey_scale = vec3((cell.r + cell.g + cell.b) / 3.0);
			COLOR = vec4(grey_scale * source * brightness, 1.0);
		}
		else {
			COLOR = vec4(texture(cell_texture, uv).rgb * sat * brightness, 1.0);
		}
	}
}
// EXAMPLE gd-script on video-player (Godot version 3.2.2)

//extends VideoPlayer
//
//func _ready() -> void:
//	# Not the most elegant way but it works.
//	var flags : = Texture.FLAG_VIDEO_SURFACE
//	flags += Texture.FLAG_MIPMAPS
//	flags += Texture.FLAG_FILTER
//	get_video_texture().set_flags(flags)
//
//	# Connecting the signal will automatically handle shader resizing.
//	connect("resized", self, "on_resize")
//	on_resize()
//
//	play()

//func on_resize() -> void:
//	material.set_shader_param("rect_size", rect_size)