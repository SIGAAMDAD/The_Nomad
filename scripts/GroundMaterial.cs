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
		if ( body is Player player && player != null ) {
			player.SetGroundMaterial( GroundMaterialType.Sand );
		}
	}

	public override void _Ready() {
		base._Ready();

		CollisionLayer = 17;
		CollisionMask = 17;

		Connect( "body_entered", Callable.From<Node2D>( OnBodyEntered ) );
		Connect( "body_exited", Callable.From<Node2D>( OnBodyExited ) );
	}
};