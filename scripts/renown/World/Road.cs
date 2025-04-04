using Godot;

public partial class Road : Node2D {
	protected Area2D ProcessArea;
	protected NavigationRegion2D NavMesh;

	public NavigationRegion2D GetNavMesh() => NavMesh;

	public override void _Ready() {
		base._Ready();

//		ProcessArea = GetNode<Area2D>( "ProcessArea" );
//		ProcessArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( ( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) => { SetProcess( true ); } ) );

		NavMesh = GetNode<NavigationRegion2D>( "NavigationRegion2D" );
	}
};
