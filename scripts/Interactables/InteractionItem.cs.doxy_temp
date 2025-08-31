using Godot;

public enum InteractionType {
	Checkpoint,
	TreasureChest,
	Tutorial,
	Note,
	Door,
	Dialogue,

	EaglesPeak,

	Count
};

public partial class InteractionItem : Area2D {
	protected CollisionShape2D InteractArea;

	protected virtual void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}
	protected virtual void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	public virtual InteractionType GetInteractionType() {
		return InteractionType.Count;
	}

	public override void _Ready() {
		base._Ready();

		InteractArea = GetNode<CollisionShape2D>( "InteractBody" );
	}
};