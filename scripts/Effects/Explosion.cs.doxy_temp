using Godot;
using Renown;

public partial class Explosion : Node2D {
	private Area2D BlowupArea;

	public float Radius = 80.0f;
	public float Damage = 30.0f;
	public Curve DamageCurve = null;
	public AmmoEntity.ExtraEffects Effects = 0;

	private void OnFinished() {
		GetParent().CallDeferred( MethodName.RemoveChild, this );
		CallDeferred( MethodName.QueueFree );
	}

	public override void _Ready() {
		base._Ready();
		
		ZIndex = 8;

		AudioStreamPlayer2D AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioStreamPlayer2D" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AudioChannel.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( OnFinished ) );

		BlowupArea = GetNode<Area2D>( "Area2D" );
		( BlowupArea.GetChild<CollisionShape2D>( 0 ).Shape as CircleShape2D ).Radius = Radius;

		CallDeferred( MethodName.CalcDamage );

		float distance = GlobalPosition.DistanceTo( GetViewport().GetCamera2D().GlobalPosition );
		if ( distance < 128.0f ) {
		}
	}

	private void CalcDamage() {
		Godot.Collections.Array<Node2D> entities = BlowupArea.GetOverlappingBodies();

		for ( int i = 0; i < (int)Scale.Length(); i++ ) {
			DebrisFactory.Create( GlobalPosition );
		}

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