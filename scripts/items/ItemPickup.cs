using Godot;
using System;

public partial class ItemPickup : Node2D {
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
		PickupArea.SetDeferred( "Monitoring", false );
		IconSprite.QueueFree();

		Reparent( body );
//		body.OnPickupItem( this );
	}

	public override void _Ready() {
		if ( Data == null ) {
			GD.PushError( "Cannot initialize ItemPickup without a valid ammo ItemDefinition" );
			QueueFree();
			return;
		}

		IconSprite = GetNode<Sprite2D>( "Icon" );
		PickupArea = GetNode<Area2D>( "PickupArea2D" );
		PickupArea.Connect( "body_shape_entered", Callable.From<Rid, Node2D, int, int>( OnPickupArea2DBodyShapeEntered ) );

		PickupSfx = GetNode<AudioStreamPlayer2D>( "PickupSfx" );

		IconSprite.Texture = (Texture2D)Data.Get( "icon" );
		PickupSfx.Stream = (AudioStream)Data.Get( "properties.pickup_sfx" );
	}
};
