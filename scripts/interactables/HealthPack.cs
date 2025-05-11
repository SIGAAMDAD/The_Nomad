using Godot;

public partial class HealthPack : InteractionItem {
	private Node Parent;
	private int NodeIndex = 0;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			player.SetHealth( player.GetHealth() + 35.0f );
			player.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/StimPack_Activate1.ogg" ) );

			GetParent().RemoveChild( this );
		}
	}

	private void OnPlayerRestart() {
		Parent.AddChild( this );
		Parent.MoveChild( this, NodeIndex );
	}

	public override void _Ready() {
		base._Ready();

		Parent = GetParent();
		NodeIndex = GetIndex();

		LevelData.Instance.PlayerRespawn += OnPlayerRestart;

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
	}
};
