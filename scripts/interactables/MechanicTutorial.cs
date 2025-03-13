using Godot;

public partial class MechanicTutorial : InteractionItem {
	private Label Text;
	private TextureRect Background;

	[Export]
	private string TutorialString;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		Text.Show();
		Background.Show();

		Player player = (Player)body;
		player.BeginInteraction( this );
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		Text.Hide();
		Background.Hide();

		Player player = (Player)body;
		player.EndInteraction();
	}

	public override InteractionType GetInteractionType() {
		return InteractionType.Tutorial;
	}

    public override void _Ready() {
		base._Ready();

		Text = GetNode<Label>( "Label" );
		Background = GetNode<TextureRect>( "TextureRect" );

		Text.Text = TutorialString;

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
