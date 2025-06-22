using Godot;

public partial class UIAudioManager : Node {
	private AudioStreamPlayer UISfxStream;
	private AudioStreamPlayer UIMusicStream;

	private void OnSettingsChanged() {
		UISfxStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		UIMusicStream.VolumeDb = SettingsData.GetMusicVolumeLinear();
	}
	
	public override void _Ready() {
		base._Ready();

		UISfxStream = new AudioStreamPlayer();
		UISfxStream.Name = "UISfxStream";
		UISfxStream.VolumeDb = SettingsData.GetEffectsVolumeLinear();
		AddChild( UISfxStream );
	}
};