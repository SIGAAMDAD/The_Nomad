using Godot;

public partial class RespawningHealthPack : InteractionItem {
	private Timer RespawnTimer;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null && player.GetHealth() < 60.0f ) {
			player.SetHealth( player.GetHealth() + 40.0f );
			player.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/StimPack_Activate1.ogg" ) );

			CallDeferred( "hide" );
			SetDeferred( "monitoring", false );
			InteractArea.SetDeferred( "disabled", true );

			RespawnTimer.Start();
		}
	}
	private void OnRespawn() {
		Show();
		Monitoring = true;
		InteractArea.Disabled = false;
	}

	public override void _Ready() {
		base._Ready();

		RespawnTimer = GetNode<Timer>( "Timer" );
		RespawnTimer.Connect( "timeout", Callable.From( OnRespawn ) );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
	}
};
