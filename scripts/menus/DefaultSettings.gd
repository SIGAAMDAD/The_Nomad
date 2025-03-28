class_name DefaultSettings extends Resource

enum WindowMode {
	Windowed,
	BorderlessWindowed,
	Fullscreen,
	BorderlessFullscreen
};

enum AntiAliasing {
	None,
	FXAA,
	MSAA_2x,
	MSAA_4x,
	MSAA_8x,
	TAA,
	FXAA_And_TAA
};

# video
@export var _window_mode:WindowMode = WindowMode.Fullscreen
@export var _vsync:DisplayServer.VSyncMode = DisplayServer.VSyncMode.VSYNC_DISABLED
@export var _antialasing:AntiAliasing = AntiAliasing.None
@export var _shadow_quality:RenderingServer.ShadowQuality = RenderingServer.ShadowQuality.SHADOW_QUALITY_HARD
@export var _bloom_enabled:bool = true
@export var _sun_shadow_enabled:bool = true
@export var _sun_shadow_quality:RenderingServer.ShadowQuality = RenderingServer.ShadowQuality.SHADOW_QUALITY_HARD

# audio
@export var _sound_effects_on:bool = true
@export var _sound_effects_volume:float = 50.0
@export var _music_on:bool = true
@export var _music_volume:float = 50.0
@export var _mute_unfocused:bool = true

# accessibility
@export var _haptic_feedback:bool = true
@export var _haptic_strength:int = 50
@export var _quicktime_autocomplete:bool = false
@export var _autoaim:bool = false
@export var _colorblind_mode:int = 0
@export var _dyslexia_mode:bool = false

# gameplay
@export var _equip_weapon_on_pickup:bool = true
@export var _draw_aim_line:bool = false
@export var _hellbreaker:bool = true
@export var _hellbreaker_revanents:bool = false

@export var _networking_enabled:bool = true
@export var _friends_only:bool = false
