using Godot;

public partial class ItemPickup : InteractionItem {
	[Export]
	public Resource Data;
	[Export]
	public int Amount = -1;

	protected Sprite2D Icon;

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
					weapon.TriggerPickup( player );
					done = true;
					break;
				case "Ammo":
					AmmoEntity ammo = new AmmoEntity();
					ammo.Name = "Ammo" + ammo;
					ammo.Data = Data;
					ammo._Ready();
					player.PickupAmmo( ammo, Amount );
					done = true;
					break;
				};
			}

			if ( done ) {
				Icon?.QueueFree();
				Icon = null;

				InteractArea.CallDeferred( "queue_free" );
			}
		}
	}

	private void Load() {
		using ( var reader = ArchiveSystem.GetSection( GetPath() ) ) {
			if ( reader == null ) {
				return;
			}
			if ( !reader.LoadBoolean( "Used" ) ) {
				CreateSprite();
			}
		}
	}
	public void Save() {
		using ( var writer = new SaveSystem.SaveSectionWriter( GetPath() ) ) {
			writer.SaveBool( "Used", Icon == null );
		}
	}

	protected void CreateSprite() {
		Icon = new Sprite2D();
		Icon.Name = "Icon";
		Icon.Texture = (Texture2D)Data.Get( "icon" );
		Icon.ZIndex = 8;
		AddChild( Icon );
	}

	public override void _Ready() {
		base._Ready();

		if ( ArchiveSystem.Instance.IsLoaded() ) {
			Load();
		} else {
			CreateSprite();
		}
		AddToGroup( "Archive" );

		Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnInteractionAreaBody2DEntered ) );
	}
};