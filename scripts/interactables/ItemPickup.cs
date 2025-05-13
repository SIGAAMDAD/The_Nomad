using Godot;

public partial class ItemPickup : InteractionItem {
	[Export]
	public Resource Data;

	private Sprite2D Icon;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		//TODO: auto-pickup toggle?
		Player player = body as Player;
		if ( player == null ) {
			return;
		}

		Godot.Collections.Array<Resource> Categories = (Godot.Collections.Array<Resource>)Data.Get( "categories" );

		bool done = false;
		for ( int i = 0; i < Categories.Count; i++ ) {
			string name = (string)Categories[i].Get( "name" );
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
			Icon.QueueFree();
			QueueFree();
		}
	}

	public override void _Ready() {
		base._Ready();

		Icon = new Sprite2D();
		Icon.Name = "Icon";
		Icon.Texture = (Texture2D)Data.Get( "icon" );
		Icon.ZIndex = 8;
		AddChild( Icon );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( "body_shape_exited", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};