using Godot;

public partial class ActionTutorial : InteractionItem {
	[Export]
	private string TutorialString;
	[Export]
	private Resource Action;

	private RichTextLabel Text;
	private Resource DialogueResource;
	private Callable Callback;

	private void OnInteract( Player player ) {
		Text.Hide();
		Player.StartThoughtBubble( "Press " + AccessibilityManager.GetBindString( Action ) + " to " + TutorialString );
		player.Disconnect( Player.SignalName.Interaction, Callback );
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Callback = Callable.From( () => OnInteract( player ) );
			Text.Show();
			player.Connect( Player.SignalName.Interaction, Callback );
			player.EmitSignal( Player.SignalName.ShowInteraction, this );
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Text.Hide();
			if ( player.IsConnected( Player.SignalName.Interaction, Callback ) ) {
				player.Disconnect( Player.SignalName.Interaction, Callback );
			}
		}
	}

	public override InteractionType GetInteractionType() {
		return InteractionType.Tutorial;
	}

    public override void _Ready() {
		base._Ready();

		Text = GetNode<RichTextLabel>( "RichTextLabel" );
		LevelData.Instance.ThisPlayer.InputMappingContextChanged += () => Text.ParseBbcode( AccessibilityManager.GetBindString( LevelData.Instance.ThisPlayer.InteractAction ) );

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
