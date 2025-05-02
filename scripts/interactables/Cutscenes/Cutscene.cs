using System.Collections.Generic;
using DialogueManagerRuntime;
using Godot;

public partial class Cutscene : Node2D {
	[Export]
	private CutsceneSequence[] Sequences;

	private Queue<CutsceneSequence> Actions = new Queue<CutsceneSequence>();

	public override void _Ready() {
		base._Ready();

		DialogueManager.DialogueEnded += OnDialogueEnded;

		for ( int i = 0; i < Sequences.Length; i++ ) {
			Actions.Enqueue( Sequences[i] );
		}
	}

	private void OnDialogueEnded( Resource dialogueResource ) {
	}
};
