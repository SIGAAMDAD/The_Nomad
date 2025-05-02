using Godot;

public enum CutsceneSequenceType : uint {
	Dialogue,
	MoveToPosition,

	Count
};

public partial class CutsceneSequence : Node2D {
	[Export]
	private CutsceneSequenceType Type;
};