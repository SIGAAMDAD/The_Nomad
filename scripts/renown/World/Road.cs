using Godot;
using System;

public partial class Road : Polygon2D {
	private Area2D ProcessArea;

	public override void _Ready() {
		base._Ready();

		ProcessArea = GetNode<Area2D>( "ProcessArea" );
		ProcessArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( ( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) => { SetProcess( true ); } ) );
	}
};
