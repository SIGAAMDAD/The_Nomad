using Godot;

public partial class HealthPack : InteractionItem {
	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null && player.GetHealth() < 60.0f ) {
			player.SetHealth( player.GetHealth() + 40.0f );
			player.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/StimPack_Activate1.ogg" ) );

			CallDeferred( "hide" );
			SetDeferred( "monitoring", false );
		}
	}

	private void OnPlayerRestart() {
		CallDeferred( "show" );
		SetDeferred( "monitoring", true );
	}

	public override void _Ready() {
		base._Ready();

		LevelData.Instance.PlayerRespawn += OnPlayerRestart;

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
	}
};
