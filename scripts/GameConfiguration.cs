using Godot;

public enum GameDifficulty {
	Easy,
	Normal,
	Hard,
	VeryHard,
	Real,

	// memes, the DNA of the soul!
	MemeMode
};

public enum GameMode {
	SinglePlayer,
	Online,
	LocalCoop2,
	LocalCoop3,
	LocalCoop4,
	Multiplayer,
	ChallengeMode,
};

public partial class GameConfiguration : Node {
	public static Node LoadedLevel = null;

	public static bool Paused = false;
	public static GameDifficulty GameDifficulty = GameDifficulty.Normal;
	public static bool DemonEyeActive = false;
	public static GameMode GameMode = GameMode.SinglePlayer;
};