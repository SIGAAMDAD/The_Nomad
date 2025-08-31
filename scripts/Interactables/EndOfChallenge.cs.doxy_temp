using Godot;

public partial class EndOfChallenge : InteractionItem {
	[Signal]
	public delegate void TriggeredEventHandler();

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}
		EmitSignalTriggered();
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}
	}
	public override void _Ready() {
		base._Ready();

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
