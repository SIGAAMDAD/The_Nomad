using Godot;
using System;

public partial class LevelEndArea : Node2D {
	[Signal]
	public delegate void LevelEndEventHandler();

	[Export]
	private Node2D MoveTo;

	private void OnArea2DBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}
		
		EmitSignal( "LevelEnd" );
	}

	public override void _Ready() {
		GetNode<Area2D>( "Area2D" ).Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnArea2DBodyShapeEntered ) );
	}
};
