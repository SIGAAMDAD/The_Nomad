using Godot;
using Renown.Thinkers;

public partial class DialogueGlobals : Node {
	/// <summary>
	/// name of the bot that's currently talking
	/// </summary>
	[Export]
	public StringName BotName;

	[Export]
	public MercenaryMaster MercMaster;

	/// <summary>
	/// the player's choice
	/// </summary>
	[Export]
	public int PlayerChoice = 0;

	[Export]
	public Thinker Entity;

	private static DialogueGlobals Instance;

	public static Player GetPlayer() => LevelData.Instance.ThisPlayer;

	public static DialogueGlobals Get() => Instance;

	public override void _EnterTree() {
		base._EnterTree();

		Instance = this;
	}
};