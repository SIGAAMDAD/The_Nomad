using DialogueManagerRuntime;
using Godot;

public partial class MechanicTutorial : InteractionItem {
	private Label Text;
	private TextureRect Background;

	[Export]
	private string TutorialString;

	private Resource DialogueResource;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		DialogueManager.ShowDialogueBalloon( DialogueResource );

		Player player = (Player)body;
		player.BeginInteraction( this );
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		Player player = (Player)body;
		player.EndInteraction();
	}

	public override InteractionType GetInteractionType() {
		return InteractionType.Tutorial;
	}

    public override void _Ready() {
		base._Ready();

		DialogueResource = DialogueManager.CreateResourceFromText( string.Format( "~ start\nMessage: {0}", TutorialString ) );

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
