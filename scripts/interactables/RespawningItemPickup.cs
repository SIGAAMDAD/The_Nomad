using Godot;

public partial class RespawningItemPickup : ItemPickup {
	[Export]
	private float CooldownTime = 15.0f;

	private Timer RespawnTimer;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		// TODO: auto-pickup toggle?
		if ( body is Player player && player != null ) {
			Godot.Collections.Array<Resource> Categories = (Godot.Collections.Array<Resource>)Data.Get( "categories" );

			bool done = false;
			for ( int i = 0; i < Categories.Count; i++ ) {
				string name = (string)Categories[ i ].Get( "name" );
				switch ( name ) {
				case "Weapon":
					WeaponEntity weapon = new WeaponEntity();
					weapon.Name = "Weapon" + weapon;
					weapon.Data = Data;
					weapon.SetResourcePath( "player/" );
					weapon.SetOwner( player );
					weapon._Ready();
					weapon.TriggerPickup( player );
					done = true;
					break;
				case "Ammo":
					AmmoEntity ammo = new AmmoEntity();
					ammo.Name = "Ammo" + ammo;
					ammo.Data = Data;
					ammo._Ready();
					player.PickupAmmo( ammo );
					done = true;
					break;
				};
			}

			if ( done ) {
				Icon.Hide();
				InteractArea.SetDeferred( "disabled", true );
				RespawnTimer.Start();
			}
		}
	}

	private void OnRespawnTimerTimeout() {
		Icon.Show();
		InteractArea.SetDeferred( "disabled", false );
	}

	public override void _Ready() {
		InteractArea = GetNode<CollisionShape2D>( "InteractBody" );

		RespawnTimer = new Timer();
		RespawnTimer.Name = "RespawnTimer";
		RespawnTimer.WaitTime = CooldownTime;
		RespawnTimer.OneShot = true;
		RespawnTimer.Connect( "timeout", Callable.From( OnRespawnTimerTimeout ) );
		AddChild( RespawnTimer );

		CreateSprite();
		MoveChild( InteractArea, GetChildCount() - 1 );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};