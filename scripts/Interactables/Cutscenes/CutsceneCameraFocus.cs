using Godot;

public partial class CutsceneCameraFocus : CutsceneSequence {
	[Export]
	private Node2D FocusNode;

	public override CutsceneSequenceType GetSequenceType() {
		return CutsceneSequenceType.Dialogue;
	}
	public override void Start() {
		
	}
};