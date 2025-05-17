using Godot;

public partial class DialogueGlobals : Node {
	/// <summary>
	/// name of the bot that's currently talking
	/// </summary>
	[Export]
	public StringName BotName;

	/// <summary>
	/// the player's choice
	/// </summary>
	[Export]
	public int PlayerChoice = 0;

	public static DialogueGlobals Instance;

	public override void _EnterTree() {
		base._EnterTree();

		Instance = this;
	}
};