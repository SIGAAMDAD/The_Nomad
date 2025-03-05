using Godot;
using System;

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

		RemoveChild( PickupSfx );
		PickupSfx.QueueFree();
		IconSprite.QueueFree();

		PickupArea.CallDeferred( "queue_free" );
		CallDeferred( "remove_child", PickupArea );

		CallDeferred( "reparent", body );
		( (Player)body ).PickupAmmo( this );
	}

	public override void _Ready() {
		if ( Data == null ) {
			GD.PushError( "Cannot initialize AmmoEntity without a valid ammo AmmoBase" );
			QueueFree();
			return;
		}

		IconSprite = GetNode<Sprite2D>( "Icon" );
		IconSprite.Texture = (Texture2D)Data.Get( "icon" );

		PickupArea = GetNode<Area2D>( "PickupArea2D" );
		PickupArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnPickupArea2DBodyShapeEntered ) );

		PickupSfx = GetNode<AudioStreamPlayer2D>( "PickupSfx" );
		PickupSfx.Stream = (AudioStream)( (Godot.Collections.Dictionary)Data.Get( "properties" ) )[ "pickup_sfx" ];
	}
};
