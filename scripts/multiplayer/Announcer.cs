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
	public static void BlueFlagReturned() => Announce( ResourceCache.GetSound( "res://sounds/announcer/blueflagreturned.ogg" ) );
	public static void BlueFlagDropped() => Announce( ResourceCache.GetSound( "res://sounds/announcer/blueflagdropped.ogg" ) );
	public static void BlueFlagTaken() => Announce( ResourceCache.GetSound( "res://sounds/announcer/blueflagtaken.ogg" ) );
	public static void BlueScores() => Announce( ResourceCache.GetSound( "res://sounds/announcer/bluescore.ogg" ) );
	public static void RedFlagReturned() => Announce( ResourceCache.GetSound( "res://sounds/announcer/redflagreturned.ogg" ) );
	public static void RedFlagDropped() => Announce( ResourceCache.GetSound( "res://sounds/announcer/redflagdropped.ogg" ) );
	public static void RedFlagTaken() => Announce( ResourceCache.GetSound( "res://sounds/announcer/redflagtaken.ogg" ) );
	public static void RedScores() => Announce( ResourceCache.GetSound( "res://sounds/announcer/redscore.ogg" ) );

	public override void _Ready() {
		base._Ready();

		AudioChannel = GetNode<AudioStreamPlayer>( "AudioStreamPlayer" );
		AudioChannel.SetProcess( false );
		AudioChannel.SetProcessInternal( false );

		SetProcess( false );
		SetProcessInternal( false );
	}
};