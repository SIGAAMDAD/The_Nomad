class_name SettingsManager extends Control

@onready var _default:DefaultSettings = preload( "res://resources/DefaultSettings.tres" )
@onready var _keybinds:KeybindResource = preload( "res://resources/KeybindsDefault.tres" )

@onready var _music_bus:int = AudioServer.get_bus_index( "Music" )
@onready var _sfx_bus:int = AudioServer.get_bus_index( "SFX" )
@onready var _ambience_bus:int = AudioServer.get_bus_index( "Ambience" )

#@onready var _resolution:Resolution = _default._resolution
@onready var _window_mode:WindowMode = _default._window_mode
@onready var _vsync:int = _default._vsync
@onready var _antialiasing:AntiAliasing = _default._antialasing
@onready var _shadow_quality:RenderingServer.ShadowQuality = _default._shadow_quality
@onready var _max_fps:int = _default._max_fps

# accessibility options
@onready var _haptic_strength:float = _default._haptic_strength
@onready var _haptic_feedback:bool = _default._haptic_feedback
@onready var _quicktime_autocomplete:bool = _default._quicktime_autocomplete
@onready var _colorblind_mode:int = _default._colorblind_mode
@onready var _autoaim:bool = _default._autoaim
@onready var _dyslexia_mode:bool = _default._dyslexia_mode

# audio options
@onready var _effects_on:bool = _default._sound_effects_on
@onready var _effects_volume:int = _default._sound_effects_volume
@onready var _music_on:bool = _default._music_on
@onready var _music_volume:int = _default._music_volume
@onready var _mute_unfocused:bool = _default._mute_unfocused

# gameplay options
@onready var _enemy_difficulty:GameConfiguration.GameDifficulty = _default._enemy_difficulty
@onready var _equip_weapon_on_pickup:bool = _default._equip_weapon_on_pickup
@onready var _draw_aim_line:bool = _default._draw_aim_line
@onready var _hellbreaker:bool = _default._hellbreaker
@onready var _hellbreaker_revanents:bool = _default._hellbreaker_revanents

var _mapping_contexts:Array[ GUIDEMappingContext ] = [ load( "res://resources/binds/binds_keyboard.tres" ), load( "res://resources/binds/binds_gamepad.tres" ) ]
var _remapper:GUIDERemapper = null
var _remapping_config:GUIDERemappingConfig = null
var _remappable_items:Array[GUIDERemapper.ConfigItem] = []
var _mapping_formatter:GUIDEInputFormatter = GUIDEInputFormatter.new()

# dedicated keybind hashmap
var _keybind_dict:Dictionary

# did we load the settings from the configuration file?
var _loaded_from_config:bool = false

enum WindowMode {
	Windowed,
	BorderlessWindowed,
	Fullscreen,
	BorderlessFullscreen
};

enum Resolution {
	Res_640x480,
	Res_800x600,
	Res_1280x720,
	Res_1600x1050,
	Res_1600x1200,
	Res_1920x1080,
	Res_2048x1152,
};

#const SCREEN_RESOLUTIONS:Dictionary = {
#	Resolution.Res_640x480: Vector2i( 640, 480 ),
#	Resolution.Res_800x600: Vector2i( 800, 600 ),
#	Resolution.Res_1280x720: Vector2i( 1280, 720 ), 
#	Resolution.Res_1600x1050: Vector2i( 1600, 1050 ),
#	Resolution.Res_1600x1200: Vector2i( 1600, 1200 ),
#	Resolution.Res_1920x1080: Vector2i( 1920, 1080 ),
#	Resolution.Res_2048x1152: Vector2i( 2048, 1152 )
#};

enum AntiAliasing {
	None,
	FXAA,
	MSAA_2x,
	MSAA_4x,
	MSAA_8x
};

func load_audio_settings( file: FileAccess ) -> void:
	_effects_on = file.get_8()
	_music_on = file.get_8()
	_effects_volume = file.get_32()
	_music_volume = file.get_32()
	
	AudioServer.set_bus_volume_db( _music_bus, _music_volume / 100.0 )
	AudioServer.set_bus_volume_db( _sfx_bus, _effects_volume / 100.0 )
	
	AudioServer.set_bus_mute( _music_bus, !_music_on )
	AudioServer.set_bus_mute( _sfx_bus, !_effects_on )

func save_audio_settings( file: FileAccess ) -> void:
	file.store_8( _effects_on )
	file.store_8( _music_on )
	file.store_32( _effects_volume )
	file.store_32( _music_volume )
	
	AudioServer.set_bus_volume_db( _music_bus, _music_volume / 100.0 )
	AudioServer.set_bus_volume_db( _sfx_bus, _effects_volume / 100.0 )
	
	AudioServer.set_bus_mute( _music_bus, !_music_on )
	AudioServer.set_bus_mute( _sfx_bus, !_effects_on )

func load_video_settings( file: FileAccess ) -> void:
#	_resolution = file.get_32()
	_window_mode = file.get_32()
	_max_fps = file.get_16()
	_shadow_quality = file.get_8()
	_antialiasing = file.get_8()
	_vsync = file.get_8()

func save_video_settings( file: FileAccess ) -> void:
#	file.store_32( _resolution )
	file.store_32( _window_mode )
	file.store_16( _max_fps )
	file.store_8( _shadow_quality )
	file.store_8( _antialiasing )
	file.store_8( _vsync )

func load_accessibility_settings( file: FileAccess ) -> void:
	_colorblind_mode = file.get_32()
	_haptic_strength = file.get_float()
	_haptic_feedback = file.get_8()
	_autoaim = file.get_8()
	_dyslexia_mode = file.get_8()

func save_accessibility_settings( file: FileAccess ) -> void:
	file.store_32( _colorblind_mode )
	file.store_float( _haptic_strength )
	file.store_8( _haptic_feedback )
	file.store_8( _autoaim )
	file.store_8( _dyslexia_mode )

func load_gameplay_settings( file: FileAccess ) -> void:
	_equip_weapon_on_pickup = file.get_8()
	_draw_aim_line = file.get_8()
	_hellbreaker = file.get_8()
	_hellbreaker_revanents = file.get_8()

func save_gameplay_settings( file: FileAccess ) -> void:
	file.store_8( _equip_weapon_on_pickup )
	file.store_8( _draw_aim_line )
	file.store_8( _hellbreaker )
	file.store_8( _hellbreaker_revanents )

func _ready() -> void:
	print( "Loading game settings..." )
	
	_remapper = GUIDERemapper.new()
	
	_remapping_config = load( "user://input_context.tres" )
	if !_remapping_config:
		_remapping_config = GUIDERemappingConfig.new()
	
	_remapper.initialize( _mapping_contexts, _remapping_config )
	
	_remappable_items = _remapper.get_remappable_items()
	
	var file := FileAccess.open( "user://settings.dat", FileAccess.READ )
	
	if file:
		_loaded_from_config = true
		load_video_settings( file )
		load_audio_settings( file )
		load_accessibility_settings(  file )
		load_gameplay_settings( file )
	else:
		save()
		file = FileAccess.open( "user://settings.dat", FileAccess.READ )
		load_video_settings( file )
		load_audio_settings( file )
		load_accessibility_settings( file )
		load_gameplay_settings( file )

func save() -> void:
	var file := FileAccess.open( "user://settings.dat", FileAccess.WRITE )
	
	save_video_settings( file )
	save_audio_settings( file )
	save_accessibility_settings( file )
	save_gameplay_settings( file )
	
	ResourceSaver.save( _remapping_config, "user://input_context.tres" )
