using DialogueManagerRuntime;
using Godot;

public partial class MechanicTutorial : InteractionItem {
	[Export]
	private string TutorialString;

	private TextureRect Background;
	private RichTextLabel InteractionPrompt;
	private Callable Callback;

	private Resource DialogueResource;

	private void OnInteract( Player player ) {
		InteractionPrompt.Hide();
		DialogueManager.ShowDialogueBalloon( DialogueResource );
		player.Disconnect( Player.SignalName.Interaction, Callback );
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Callback = Callable.From( () => OnInteract( player ) );
			InteractionPrompt.Show();
			player.Connect( Player.SignalName.Interaction, Callback );
			player.EmitSignal( Player.SignalName.ShowInteraction, this );
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			InteractionPrompt.Hide();
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

		DialogueResource = DialogueManager.CreateResourceFromText( string.Format( "~ start\nTUTORIAL: {0}", TutorialString ) );

		InteractionPrompt = GetNode<RichTextLabel>( "RichTextLabel" );
		LevelData.Instance.ThisPlayer.InputMappingContextChanged += () => InteractionPrompt.ParseBbcode( AccessibilityManager.GetBindString( LevelData.Instance.ThisPlayer.InteractAction ) );

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
