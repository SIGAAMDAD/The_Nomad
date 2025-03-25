using Godot;

public enum GameDifficulty {
	// the actual experience
	Intended,
	
	// just let 'em rip & tear...
	PowerFantasy,

	// memes, the DNA of the soul!
	MemeMode
};

public enum GameMode {
	SinglePlayer,
	Online,
	LocalCoop2,
	LocalCoop3,
	LocalCoop4,
	Multiplayer
};

public partial class GameConfiguration : Node {
	public static Node LoadedLevel = null;

	public static bool Paused = false;
	public static GameDifficulty GameDifficulty = GameDifficulty.Intended;
	public static bool DemonEyeActive = false;
	public static GameMode GameMode = GameMode.SinglePlayer;
};