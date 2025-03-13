using Godot;

public partial class ActionTutorial : InteractionItem {
	private RichTextLabel Text;
	private TextureRect Background;

	[Export]
	private string TutorialString;
	[Export]
	private Resource Action;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		Player player = (Player)body;

		Node SettingsData = GetNode( "/root/SettingsData" );
		Godot.Collections.Array<RefCounted> items = (Godot.Collections.Array<RefCounted>)( (RefCounted)SettingsData.Get( "_remapper" ) ).Call( "get_remappable_items", player.GetCurrentMappingContext(), "", Action );
		Resource input = (Resource)( (RefCounted)SettingsData.Get( "_remapper" ) ).Call( "get_bound_input_or_null", items[0] );
		if ( input == null ) {
			Text.Text = "Press [color=red]UNBOUND[/color] to " + TutorialString;
		} else {
			Text.ParseBbcode( "Press " + (string)( (RefCounted)SettingsData.Get( "_mapping_formatter" ) ).Call( "input_as_richtext_async", input ) + " to " + TutorialString );
		}

		Text.Show();
		Background.Show();

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

		Text = GetNode<RichTextLabel>( "RichTextLabel" );
		Background = GetNode<TextureRect>( "TextureRect" );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
