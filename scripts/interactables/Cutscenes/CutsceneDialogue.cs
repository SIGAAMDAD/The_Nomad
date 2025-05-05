using DialogueManagerRuntime;
using Godot;

public partial class CutsceneDialogue : CutsceneSequence {
	[Export]
	private Resource DialogueResource;
	[Export]
	private StringName Starting;

	private void OnDialogueEnded( Resource dialogueResource ) {
		DialogueManager.DialogueEnded -= OnDialogueEnded;
		EmitSignalEnd();
	}

	public override CutsceneSequenceType GetSequenceType() {
		return CutsceneSequenceType.Dialogue;
	}
	public override void Start() {
		DialogueManager.DialogueEnded += OnDialogueEnded;

		DialogueManager.ShowDialogueBalloon( DialogueResource, Starting );
	}
};