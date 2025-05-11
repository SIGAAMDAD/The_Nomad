using Godot;
using Renown;

public partial class Explosion : Node2D {
	private Area2D BlowupArea;

	public float Damage = 30.0f;
	public Curve DamageCurve = null;
	public AmmoEntity.ExtraEffects Effects = 0;

	private void OnFinished() {
		GetParent().CallDeferred( "remove_child", this );
		CallDeferred( "queue_free" );
	}

	public override void _Ready() {
		base._Ready();
		
		ZIndex = 8;

		for ( int i = 0; i < (int)Scale.Length(); i++ ) {
			DebrisFactory.Create( GlobalPosition );
		}

		AudioStreamPlayer2D AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioStreamPlayer2D" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AudioChannel.Connect( "finished", Callable.From( OnFinished ) );

		CircleShape2D CircleShape = new CircleShape2D();
		if ( Scale.Length() == 1.0f ) {
			CircleShape.Radius = 30.0f;
		} else {
			CircleShape.Radius = Scale.Length();
		}

		CollisionShape2D Collision = new CollisionShape2D();
		Collision.Name = "Collision";
		Collision.Shape = CircleShape;

		BlowupArea = new Area2D();
		BlowupArea.Name = "BlowupArea";
		BlowupArea.CollisionLayer = 1 | 2 | 5 | 8 | 9;
		BlowupArea.CollisionMask = 1 | 2 | 5 | 8 | 9;
		BlowupArea.AddChild( Collision );
		AddChild( BlowupArea );

		CallDeferred( "CalcDamage" );

		float distance = GlobalPosition.DistanceTo( GetViewport().GetCamera2D().GlobalPosition );
		if ( distance < 128.0f ) {
		}
	}

	private void CalcDamage() {
		Godot.Collections.Array<Node2D> entities = BlowupArea.GetOverlappingBodies();
		for ( int i = 0; i < entities.Count; i++ ) {
			if ( entities[i] is Entity entity && entity != null ) {
				float damage = Damage * ( DamageCurve != null ? DamageCurve.SampleBaked( entity.GlobalPosition.DistanceTo( GlobalPosition ) ) : 1.0f );
				entity.Damage( GetParent<Entity>(), damage );
				if ( ( Effects & AmmoEntity.ExtraEffects.Incendiary ) != 0 ) {
					entity.AddStatusEffect( "status_burning" );
				}
			}
		}
	}
};