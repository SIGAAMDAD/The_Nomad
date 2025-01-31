extends Node

# some settings in the main menu that shouldn't be released once we're
# in a level end up here where we can access them through a singleton

var LoadedLevel:AsyncScene = null

enum GameDifficulty {
	Easy,
	Normal,
	Hard,
	VeryHard,
	Insane,
	MemeMode
};

enum DirType {
	North,
	East,
	South,
	West
};

enum GameMode {
	Singleplayer,
	Coop2,
	Coop3,
	Coop4
};

var _mute_unfocused:bool = true
var _game_difficulty:GameDifficulty = GameDifficulty.Easy

var _in_level:bool = false

func _process( _delta: float ) -> void:
	if !_in_level:
		return
	
#	if _mute_unfocused:
#		_sfx_bus.mute = !DisplayServer.window_is_focused()
#		_music_bus.mute = !DisplayServer.window_is_focused()
#	else:
#		_sfx_bus.mute = false
#		_music_bus.mute = false
