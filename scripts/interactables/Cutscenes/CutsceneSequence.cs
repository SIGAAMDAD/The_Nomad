using Godot;

public enum CutsceneSequenceType : uint {
	Dialogue,
	MoveToPosition,

	Count
};

public partial class CutsceneSequence : Node2D {
	[Signal]
	public delegate void EndEventHandler();

	public virtual CutsceneSequenceType GetSequenceType() {
		return CutsceneSequenceType.Count;
	}
	public virtual void Start() {
	}
};