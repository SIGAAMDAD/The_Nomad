using Godot;

public enum GroundMaterialType : int {
	Stone,
	Sand,
	Wood,
	Water,

	Count
};

public partial class GroundMaterial : Area2D {
	[Export]
	private GroundMaterialType Type;

	private void OnBodyEntered( Node2D body ) {
		if ( body is Player player && player != null ) {
			player.SetGroundMaterial( Type );
		}
	}
	private void OnBodyExited( Node2D body ) {
	}

	public override void _Ready() {
		base._Ready();

		Connect( "body_entered", Callable.From<Node2D>( OnBodyEntered ) );
		Connect( "body_exited", Callable.From<Node2D>( OnBodyExited ) );
//		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, bodyShapeIndex, localShapeIndex ) => { OnBodyEntered( body ); } ) );
//		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( ( bodyRid, body, bodyShapeIndex, localShapeIndex ) => { OnBodyExited( body ); } ) );
	}
};