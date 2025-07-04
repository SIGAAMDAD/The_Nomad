using DialogueManagerRuntime;
using Godot;

public partial class Note : InteractionItem {
	[Export]
	public StringName TextId { get; private set; }

	private Resource DialogueResource;
	private Callable Callback;
	private RichTextLabel Text;

	private void OnInteraction( Player player ) {
		player.Disconnect( "Interaction", Callback );
		player.AddToJournal( this );

		DialogueManager.ShowDialogueBalloon( DialogueResource );
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Callback = Callable.From( () => OnInteraction( player ) );
			player.Connect( "Interaction", Callback );
			Text.Show();
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			Text.Hide();
			if ( player.IsConnected( "Interaction", Callback ) ) {
				player.Disconnect( "Interaction", Callback );
			}
		}
	}

	public override InteractionType GetInteractionType() {
		return InteractionType.Note;
	}

	public override void _Ready() {
		base._Ready();

		Text = GetNode<RichTextLabel>( "RichTextLabel" );
		LevelData.Instance.ThisPlayer.InputMappingContextChanged += () => Text.ParseBbcode( AccessibilityManager.GetBindString( LevelData.Instance.ThisPlayer.InteractAction ) );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );

		DialogueResource = DialogueManager.CreateResourceFromText( "~ start\nNote: " + TranslationServer.Translate( TextId ) );
	}
};