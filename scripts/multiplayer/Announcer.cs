using Godot;

public partial class Announcer : Node {
	private static AudioStreamPlayer AudioChannel;

	private static void Announce( AudioStream audio ) {
		AudioChannel.Stream = audio;
		AudioChannel.Play();
	}

	public static void PrepareToFight() => Announce( ResourceCache.GetSound( "res://sounds/announcer/prepare.wav" ) );
	public static void TakenLead() => Announce( ResourceCache.GetSound( "res://sounds/announcer/takenlead.wav" ) );
	public static void TiedLead() => Announce( ResourceCache.GetSound( "res://sounds/announcer/tiedlead.wav" ) );
	public static void LostLead() => Announce( ResourceCache.GetSound( "res://sounds/announcer/lostlead.wav" ) );
	public static void Fight() => Announce( ResourceCache.GetSound( "res://sounds/announcer/fight.wav" ) );

	public override void _Ready() {
		base._Ready();

		AudioChannel = GetNode<AudioStreamPlayer>( "AudioStreamPlayer" );
		AudioChannel.SetProcess( false );
		AudioChannel.SetProcessInternal( false );

		SetProcess( false );
		SetProcessInternal( false );
	}
};