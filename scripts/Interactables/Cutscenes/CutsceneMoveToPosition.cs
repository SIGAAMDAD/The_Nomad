using Godot;
using Renown;

public partial class CutsceneMoveToPosition : CutsceneSequence {
	[Export]
	private Node2D MoveTo;
	[Export]
	private Entity MoveTarget;

	private Vector2 MoveSpeed = Vector2.Zero;
	private Vector2 MoveDirection = Vector2.Zero;

	public override CutsceneSequenceType GetSequenceType() {
		return CutsceneSequenceType.MoveToPosition;
	}
	public override void Start() {
		MoveDirection = MoveTarget.GlobalPosition.DirectionTo( MoveTo.GlobalPosition );
	}

	public override void _PhysicsProcess( double delta ) {
		base._PhysicsProcess( delta );

		MoveTarget.Velocity = Player.MAX_SPEED * MoveDirection;
		MoveTarget.MoveAndSlide();

		if ( MoveTarget.GlobalPosition.DistanceTo( MoveTo.GlobalPosition ) < 10.0f ) {
			SetPhysicsProcess( false );
			EmitSignalEnd();
		}
	}
};