using Godot;

public partial class HealthPack : InteractionItem {
	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null && player.GetHealth() < 60.0f ) {
			player.SetHealth( player.GetHealth() + 40.0f );
			player.PlaySound( null, ResourceCache.GetSound( "res://sounds/env/StimPack_Activate1.ogg" ) );

			CallDeferred( MethodName.Hide );
			SetDeferred( PropertyName.Monitoring, false );
		}
	}

	private void OnPlayerRestart() {
		CallDeferred( MethodName.Show );
		SetDeferred( PropertyName.Monitoring, true );
	}

	public override void _Ready() {
		base._Ready();

		LevelData.Instance.PlayerRespawn += OnPlayerRestart;

		Connect( SignalName.AreaShapeEntered, Callable.From<Rid, Area2D, int, int>( ( bodyRid, area, areaShapeIndex, localShapeIndex ) => OnInteractionAreaBody2DEntered( bodyRid, area, areaShapeIndex, localShapeIndex ) ) );
		Connect( SignalName.AreaShapeExited, Callable.From<Rid, Area2D, int, int>( ( bodyRid, area, areaShapeIndex, localShapeIndex ) => OnInteractionAreaBody2DExited( bodyRid, area, areaShapeIndex, localShapeIndex ) ) );
		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};
