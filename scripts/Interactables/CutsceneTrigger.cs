using Godot;

public partial class CutsceneTrigger : InteractionItem {
	[Export]
	private Cutscene Cutscene;
	[Export]
	private bool OneShot = false;

	private bool Triggered = false;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			if ( OneShot && Triggered ) {
				return;
			}
			Triggered = true;
			Cutscene.Start();
			player.BlockInput( true );
			Cutscene.Finished += () => { player.BlockInput( false ); };
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	public override void _Ready() {
		base._Ready();

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
