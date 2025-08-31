using Godot;

public partial class UsedItemPickup : InteractionItem {
	public Node2D Entity = null;
	public int Amount = 0;

	protected Sprite2D Icon;

	protected override void OnInteractionAreaBody2DEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is Player player && player != null ) {
			if ( Entity is WeaponEntity weapon && weapon != null ) {
				weapon.TriggerPickup( player );
			} else if ( Entity is AmmoEntity ammo && ammo != null ) {
				ammo._Ready();
				player.PickupAmmo( ammo, Amount );
			}
		}
	}

	public void Load( SaveSystem.SaveSectionReader reader ) {
	}
	public void Save( SaveSystem.SaveSectionWriter writer, int index ) {
	}

	protected void CreateSprite() {
		Icon = new Sprite2D();
		Icon.Name = "Icon";
		if ( Entity is AmmoEntity ammo && ammo != null ) {
			Icon.Texture = (Texture2D)ammo.Data.Get( "icon" );
		} else if ( Entity is WeaponEntity weapon && weapon != null ) {
			Icon.Texture = weapon.Icon;
		}
		Icon.ZIndex = 8;
		AddChild( Icon );
	}

	public void AdjustPosition() {
		Godot.Vector2 position = GlobalPosition;
		position.X += 10.0f;
		GlobalPosition = position;
	}

	public override void _Ready() {
		base._Ready();

		if ( !ArchiveSystem.Instance.IsLoaded() ) {
			CreateSprite();
		}

		Connect( SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
		Connect( SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DExited ) );
	}
};