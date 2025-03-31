using Godot;

public partial class AmmoEntity : Node2D {
	public enum Type {
		Heavy,
		Light,
		Pellets
	};

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
	private AudioStreamPlayer2D PickupSfx;
	private Sprite2D IconSprite;

	private void OnPickupArea2DBodyShapeEntered( Rid bodyRID, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
		if ( body is not Player ) {
			return;
		}

		PickupSfx.Play();

		IconSprite.QueueFree();

		PickupArea.CallDeferred( "remove_child", PickupArea.GetChild( 0 ) );
		PickupArea.GetChild( 0 ).CallDeferred( "queue_free" );
		PickupArea.CallDeferred( "queue_free" );
		CallDeferred( "remove_child", PickupArea );

		CallDeferred( "reparent", body );
		( (Player)body ).PickupAmmo( this );
	}
	private void OnPickupSfxFinished() {
		CallDeferred( "remove_child", PickupSfx );
		PickupSfx.QueueFree();
	}

	public override void _Ready() {
		if ( Data == null ) {
			GD.PushError( "Cannot initialize AmmoEntity without a valid ammo AmmoBase" );
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

		PickupSfx = GetNode<AudioStreamPlayer2D>( "PickupSfx" );
		PickupSfx.SetProcess( false );
		PickupSfx.SetProcessInternal( false );
		PickupSfx.Connect( "finished", Callable.From( OnPickupSfxFinished ) );
		PickupSfx.Stream = (AudioStream)( (Godot.Collections.Dictionary)Data.Get( "properties" ) )[ "pickup_sfx" ];
	}
};
