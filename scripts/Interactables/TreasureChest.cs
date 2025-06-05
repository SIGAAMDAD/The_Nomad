using Godot;

public partial class TreasureChest : InteractionItem {
	[Export]
	private Godot.Collections.Array<Resource> Items;
	[Export]
	private Resource LockItem; // if it's locked, then it requires this item to use

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}
	}
	protected override void OnInteractionAreaBody2DExited( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
	}

	public override InteractionType GetInteractionType() {
		return InteractionType.TreasureChest;
	}

	public override void _Ready() {
		base._Ready();
	}
};
