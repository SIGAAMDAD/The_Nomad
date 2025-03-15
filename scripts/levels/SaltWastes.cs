using Godot;
using NathanHoad;

public partial class SaltWastes : Node2D {
	private AudioStream Ambience;

	public override void _Ready() {
		base._Ready();

		Ambience = ResourceLoader.Load<AudioStream>( "res://music/Ambience/desert_ambience.ogg" );
		SoundManager.PlayAmbientSound( Ambience, 0.25f, "Ambience" );
	}
};
