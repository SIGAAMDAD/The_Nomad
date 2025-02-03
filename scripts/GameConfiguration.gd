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
	Coop4,
	Multiplayer
};

var _mute_unfocused:bool = true
var _game_difficulty:GameDifficulty = GameDifficulty.Easy

var _in_level:bool = false

var _game_mode:GameMode = GameMode.Singleplayer
var _hosting_lobby: bool = false
