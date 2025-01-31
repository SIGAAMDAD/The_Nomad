extends Control

@onready var _vsync:CheckBox = $TabContainer/Video/VBoxContainer/VSyncButton/VSyncCheckBox
@onready var _resolution:OptionButton = $TabContainer/Video/VBoxContainer/ResolutionList/ResolutionOptionButton
@onready var _window_mode:OptionButton = $TabContainer/Video/VBoxContainer/WindowModeList/WindowModeOptionButton
@onready var _antialiasing:OptionButton = $TabContainer/Video/VBoxContainer/AntiAliasingList/AntiAliasingOptionButton
@onready var _shadow_quality:OptionButton = $TabContainer/Video/VBoxContainer/ShadowQualityList/ShadowQualityOptionButton

@onready var _effects_on:CheckBox = $TabContainer/Audio/VBoxContainer/EffectsOnButton/EffectsOnCheckBox
@onready var _effects_volume:HSlider = $TabContainer/Audio/VBoxContainer/EffectsVolumeSlider/EffectsVolumeHSlider
@onready var _music_on:CheckBox = $TabContainer/Audio/VBoxContainer/MusicOnButton/MusicOnCheckBox
@onready var _music_volume:HSlider = $TabContainer/Audio/VBoxContainer/MusicVolumeSlider/MusicVolumeHSlider
@onready var _mute_unfocused:CheckBox = $TabContainer/Audio/VBoxContainer/MuteOnUnfocusedButton/MuteOnUnfocusedCheckBox

@onready var _haptic_enabled:CheckBox = $TabContainer/Accessibility/VBoxContainer/HapticFeedbackButton/HapticFeedbackCheckbox
@onready var _haptic_strength:HSlider = $TabContainer/Accessibility/VBoxContainer/HapticStrengthSlider/HapticStrengthSlider
@onready var _autoaim_enabled:CheckBox = $TabContainer/Accessibility/VBoxContainer/AutoAimButton/AutoAimCheckbox
@onready var _dyslexia_mode:CheckBox = $TabContainer/Accessibility/VBoxContainer/DyslexiaModeButton/DyslexiaCheckbox

func update_window_scale() -> void:
	var centerScreen = DisplayServer.screen_get_position() + DisplayServer.screen_get_size() / 2
	var windowSize = get_window().get_size_with_decorations()
	get_window().set_ime_position( centerScreen - windowSize / 2 )

func apply_video_settings() -> void:
	if _vsync.button_pressed:
		DisplayServer.window_set_vsync_mode( DisplayServer.VSYNC_ADAPTIVE )
	else:
		DisplayServer.window_set_vsync_mode( DisplayServer.VSYNC_DISABLED )
	
	# from https://www.reddit.com/r/godot/comments/xvgl3r/how_to_change_graphic_settings_on_runtime_godot/
	var viewport := get_tree().get_root().get_viewport_rid()
	match _antialiasing.selected:
		SettingsManager.AntiAliasing.None:
			RenderingServer.viewport_set_use_taa( viewport, false )
			RenderingServer.viewport_set_screen_space_aa( viewport, RenderingServer.VIEWPORT_SCREEN_SPACE_AA_DISABLED )
			RenderingServer.viewport_set_msaa_2d( viewport, RenderingServer.VIEWPORT_MSAA_DISABLED )
		SettingsManager.AntiAliasing.FXAA:
			RenderingServer.viewport_set_use_taa( viewport, false )
			RenderingServer.viewport_set_screen_space_aa( viewport, RenderingServer.VIEWPORT_SCREEN_SPACE_AA_FXAA )
			RenderingServer.viewport_set_msaa_2d( viewport, RenderingServer.VIEWPORT_MSAA_DISABLED )
		SettingsManager.AntiAliasing.MSAA_2x:
			RenderingServer.viewport_set_use_taa( viewport, false )
			RenderingServer.viewport_set_screen_space_aa( viewport, RenderingServer.VIEWPORT_SCREEN_SPACE_AA_DISABLED )
			RenderingServer.viewport_set_msaa_2d( viewport, RenderingServer.VIEWPORT_MSAA_2X )
		SettingsManager.AntiAliasing.MSAA_4x:
			RenderingServer.viewport_set_use_taa( viewport, false )
			RenderingServer.viewport_set_screen_space_aa( viewport, RenderingServer.VIEWPORT_SCREEN_SPACE_AA_DISABLED )
			RenderingServer.viewport_set_msaa_2d( viewport, RenderingServer.VIEWPORT_MSAA_4X )
		SettingsManager.AntiAliasing.MSAA_8x:
			RenderingServer.viewport_set_use_taa( viewport, false )
			RenderingServer.viewport_set_screen_space_aa( viewport, RenderingServer.VIEWPORT_SCREEN_SPACE_AA_DISABLED )
			RenderingServer.viewport_set_msaa_2d( viewport, RenderingServer.VIEWPORT_MSAA_8X )
		SettingsManager.AntiAliasing.TAA:
			RenderingServer.viewport_set_use_taa( viewport, true )
			RenderingServer.viewport_set_screen_space_aa( viewport, RenderingServer.VIEWPORT_SCREEN_SPACE_AA_DISABLED )
			RenderingServer.viewport_set_msaa_2d( viewport, RenderingServer.VIEWPORT_MSAA_DISABLED )
	
	RenderingServer.positional_soft_shadow_filter_set_quality( _shadow_quality.selected )
	
	match _window_mode.selected:
		SettingsManager.WindowMode.Windowed:
			DisplayServer.window_set_mode( DisplayServer.WINDOW_MODE_WINDOWED )
			DisplayServer.window_set_flag( DisplayServer.WINDOW_FLAG_BORDERLESS, false )
		SettingsManager.WindowMode.BorderlessWindowed:
			DisplayServer.window_set_mode( DisplayServer.WINDOW_MODE_WINDOWED )
			DisplayServer.window_set_flag( DisplayServer.WINDOW_FLAG_BORDERLESS, true )
		SettingsManager.WindowMode.Fullscreen:
			DisplayServer.window_set_mode( DisplayServer.WINDOW_MODE_FULLSCREEN )
			DisplayServer.window_set_flag( DisplayServer.WINDOW_FLAG_BORDERLESS, false )
		SettingsManager.WindowMode.BorderlessFullscreen:
			DisplayServer.window_set_mode( DisplayServer.WINDOW_MODE_FULLSCREEN )
			DisplayServer.window_set_flag( DisplayServer.WINDOW_FLAG_BORDERLESS, true )
	
	const RESOLUTION_DICT := {
		SettingsManager.Resolution.Res_1280x720: Vector2i( 1280, 720 ),
		SettingsManager.Resolution.Res_1600x1050: Vector2i( 1600, 1050 ),
		SettingsManager.Resolution.Res_1600x1200: Vector2i( 1600, 1200 ),
		SettingsManager.Resolution.Res_1920x1080: Vector2i( 1920, 1080 )
	};
	
	DisplayServer.window_set_size( RESOLUTION_DICT[ _resolution.selected ] )
	update_window_scale()

func _ready() -> void:
	_vsync.button_pressed = SettingsData._vsync
	_resolution.selected = SettingsData._resolution
	_window_mode.selected = SettingsData._window_mode
	
	_effects_on.button_pressed = SettingsData._effects_on
	_effects_volume.value = SettingsData._effects_volume
	_music_on.button_pressed = SettingsData._music_on
	_music_volume.value = SettingsData._music_volume
	
	_dyslexia_mode.button_pressed = SettingsData._dyslexia_mode
	_haptic_enabled.button_pressed = SettingsData._haptic_feedback
	_haptic_strength.value = SettingsData._haptic_strength
	
	apply_video_settings()

func _on_save_settings_button_pressed() -> void:
	SettingsData._vsync = _vsync.button_pressed
	SettingsData._window_mode = _window_mode.selected
	SettingsData._resolution = _resolution.selected
	
	SettingsData._effects_on = _effects_on.button_pressed
	SettingsData._music_on = _music_on.button_pressed
	SettingsData._effects_volume = _effects_volume.value
	SettingsData._music_volume = _music_volume.value
	SettingsData._mute_unfocused = _mute_unfocused.button_pressed
	
	SettingsData._haptic_feedback = _haptic_enabled.button_pressed
	SettingsData._haptic_strength = _haptic_strength.value
	SettingsData._autoaim = _autoaim_enabled.button_pressed
	SettingsData._dyslexia_mode = _dyslexia_mode.button_pressed
	
	apply_video_settings()
	
	SettingsData.save()
