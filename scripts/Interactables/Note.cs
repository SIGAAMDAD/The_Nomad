using DialogueManagerRuntime;
using Godot;

public partial class Note : InteractionItem {
	[Export]
	private StringName TextId;

	private Resource DialogueResource;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		DialogueManager.ShowDialogueBalloon( DialogueResource );
	}
	public override InteractionType GetInteractionType() {
		return InteractionType.Note;
	}

	public override void _Ready() {
		base._Ready();

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );

		DialogueResource = DialogueManager.CreateResourceFromText( "~ start\nNote: " + TranslationServer.Translate( TextId ) );
	}
};