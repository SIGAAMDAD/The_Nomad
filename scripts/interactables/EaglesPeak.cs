using Godot;

public partial class EaglesPeak : InteractionItem {
	[Export]
	private Texture2D ViewImage;
	[Export]
	private AudioStream Music;
	[Export]
	private Checkpoint Destination;
	
	private Player Interactor = null;

	public Texture2D GetViewImage() => ViewImage;
	public AudioStream GetMusic() => Music;

	private void OnTransitionFinished() {
		RemoveChild( GetChild( GetChildCount() - 1 ) );
	}
	public void OnYesButtonPressed() {
		AudioStreamPlayer audio = new AudioStreamPlayer();
		AddChild( audio );
		audio.Stream = AudioCache.LeapOfFaithSfx;
		audio.Connect( "finished", Callable.From( () => {
			RemoveChild( audio );
			audio.QueueFree();
		} ) );
		audio.Play();

		CanvasLayer transition = ResourceLoader.Load<PackedScene>( "res://scenes/menus/fade.tscn" ).Instantiate<CanvasLayer>();
		AddChild( transition );
		transition.Call( "transition" );
		transition.Connect( "transition_finished", Callable.From( OnTransitionFinished ) );

		Interactor.GlobalPosition = Destination.GlobalPosition;
	}
	public void OnNoButtonPressed() {
		Interactor.EndInteraction();
	}

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		Interactor = body as Player;
		if ( Interactor == null ) {
			return;
		}
		Interactor.BeginInteraction( this );
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		Player player = body as Player;
		if ( player == null ) {
			return;
		}
		player.EndInteraction();

		Interactor = null;
	}
	
	public override InteractionType GetInteractionType() {
		return InteractionType.EaglesPeak;
	}

    public override void _Ready() {
		base._Ready();
		
		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
