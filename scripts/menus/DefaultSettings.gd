class_name DefaultSettings extends Resource

# video
@export var _window_mode:SettingsManager.WindowMode = SettingsManager.WindowMode.Fullscreen
@export var _vsync:bool = false
@export var _resolution:SettingsManager.Resolution = SettingsManager.Resolution.Res_1280x720
@export var _antialasing:SettingsManager.AntiAliasing = SettingsManager.AntiAliasing.None
@export var _shadow_quality:RenderingServer.ShadowQuality = RenderingServer.ShadowQuality.SHADOW_QUALITY_HARD
@export var _max_fps:int = 60

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
@export var _enemy_difficulty:GameConfiguration.GameDifficulty = GameConfiguration.GameDifficulty.Normal
@export var _equip_weapon_on_pickup:bool = true
