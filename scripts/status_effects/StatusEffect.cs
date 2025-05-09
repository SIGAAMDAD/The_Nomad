using Godot;
using Renown;

public partial class StatusEffect : Node2D {
	[Export]
	protected float Duration = 0.0f;

	protected Timer EffectTimer;
	protected AudioStreamPlayer2D AudioChannel;
	protected Entity Victim;

	[Signal]
	public delegate void TimeoutEventHandler();

	public virtual void SetVictim( Entity owner ) {
		Victim = owner;
	}
	public virtual void ResetTimer() {
		EffectTimer.Start();
	}
	public virtual float GetTimeLeft() {
		return (float)EffectTimer.TimeLeft;
	}

	public override void _Ready() {
		base._Ready();

		AudioChannel = GetNode<AudioStreamPlayer2D>( "AudioChannel" );
		AudioChannel.VolumeDb = SettingsData.GetEffectsVolumeLinear();

		EffectTimer = GetNode<Timer>( "EffectTimer" );
		EffectTimer.Autostart = true;
		EffectTimer.Connect( "timeout", Callable.From( EmitSignalTimeout ) );
	}
};