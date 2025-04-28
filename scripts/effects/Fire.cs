using Godot;

public partial class Fire : CpuParticles2D {
	public override void _Ready() {
		base._Ready();

		AudioStreamPlayer2D stream = GetNode<AudioStreamPlayer2D>( "AudioStreamPlayer2D" );
		stream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
	}
};
