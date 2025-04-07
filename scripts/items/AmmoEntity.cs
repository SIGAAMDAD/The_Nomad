using Godot;

public partial class AmmoEntity : Node2D {
	public enum ShotgunBullshit {
		Flechette,
		Buckshot,
		Birdshot,
		Shrapnel,
		Slug,

		None
	};

	[Export]
	public Resource Data = null;

	private Area2D PickupArea;
	private Sprite2D IconSprite;

	private AudioStream PickupSfx;
	private float Damage;
	private AmmoType AmmoType;

	public AudioStream GetPickupSound() => PickupSfx;
	public AmmoType GetAmmoType() => AmmoType;
	public float GetDamage() => Damage;

	private void OnPickupArea2DBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player player ) {
			return;
		}

		IconSprite.QueueFree();

		PickupArea.CallDeferred( "remove_child", PickupArea.GetChild( 0 ) );
		PickupArea.GetChild( 0 ).CallDeferred( "queue_free" );
		PickupArea.CallDeferred( "queue_free" );
		CallDeferred( "remove_child", PickupArea );

		CallDeferred( "reparent", body );
		player.PickupAmmo( this );
	}

	public override void _Ready() {
		if ( Data == null ) {
			Console.PrintError( "Cannot initialize AmmoEntity without a valid ammo AmmoBase" );
			QueueFree();
			return;
		}

		IconSprite = GetNode<Sprite2D>( "Icon" );
		IconSprite.SetProcess( false );
		IconSprite.SetProcessInternal( false );
		IconSprite.Texture = (Texture2D)Data.Get( "icon" );

		PickupArea = GetNode<Area2D>( "PickupArea2D" );
		PickupArea.SetProcess( false );
		PickupArea.SetProcessInternal( false );
		PickupArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnPickupArea2DBodyShapeEntered ) );

		Godot.Collections.Dictionary properties = (Godot.Collections.Dictionary)Data.Get( "properties" );

		PickupSfx = (AudioStream)properties[ "pickup_sfx" ];
		Damage = (float)properties[ "damage" ];
		AmmoType = (AmmoType)(uint)properties[ "type" ];
	}
};
