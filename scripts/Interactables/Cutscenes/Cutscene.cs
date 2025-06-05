using System.Collections.Generic;
using Godot;

public partial class Cutscene : Node2D {
	[Export]
	private CutsceneSequence[] Sequences;
	[Export]
	private CutsceneTrigger Trigger;

	private Queue<CutsceneSequence> Actions = new Queue<CutsceneSequence>();
	private CutsceneSequence Sequence;

	[Signal]
	public delegate void FinishedEventHandler();

	public override void _Ready() {
		base._Ready();

		for ( int i = 0; i < Sequences.Length; i++ ) {
			Actions.Enqueue( Sequences[i] );
			Sequences[i].End += OnSequenceEnded;
		}
	}

	public void Start() {
		Actions.TryDequeue( out Sequence );
		Sequence.Start();
	}

	private void OnSequenceEnded() {
		if ( Actions.TryDequeue( out Sequence ) ) {
			Sequence.Start();
		}
		EmitSignalFinished();
	}
};
