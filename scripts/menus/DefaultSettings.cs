using Godot;

public enum WindowMode : uint {
	Windowed,
	BorderlessWindowed,
	Fullscreen,
	BorderlessFullscreen
};

public enum AntiAliasing : uint {
	None,
	FXAA,
	MSAA_2x,
	MSAA_4x,
	MSAA_8x,
	TAA,
	FXAA_and_TAA
};

public enum ShadowQuality : uint {
	Off,
	NoFilter,
	Low,
	High,

	Count
};

public enum VSyncMode : uint {
	Off,
	Adaptive,
	On,
	TripleBuffered
};

public enum SaveMode : uint {
	Checkpoint_And_Die,
	Anytime
};

public partial class DefaultSettings : Resource {
	//
	// video
	//
	[Export]
	public WindowMode WindowMode { get; private set; } = WindowMode.Fullscreen;
	[Export]
	public VSyncMode Vsync { get; private set; } = VSyncMode.Off;
	[Export]
	public AntiAliasing AntiAliasing { get; private set; } = AntiAliasing.None;
	[Export]
	public ShadowQuality ShadowQuality { get; private set; } = ShadowQuality.NoFilter;
	[Export]
	public int MaxFps { get; private set; } = 60;
	[Export]
	public bool BloomEnabled { get; private set; } = true;
	[Export]
	public bool SunLightEnabled { get; private set; } = true;
	[Export]
	public ShadowQuality SunShadowQuality { get; private set; } = ShadowQuality.NoFilter;
	[Export]
	public bool ShowFps { get; private set; } = false;
	[Export]
	public bool ShowBlood { get; private set; } = true;

	//
	// audio
	//
	[Export]
	public bool SoundEffectsOn { get; private set; } = true;
	[Export]
	public float SoundEffectsVolume { get; private set; } = 50.0f;
	[Export]
	public bool MusicOn { get; private set; } = true;
	[Export]
	public float MusicVolume { get; private set; } = 50.0f;
	[Export]
	public bool MuteUnfocused { get; private set; } = true;

	//
	// accessibility
	//
	[Export]
	public bool HapticFeedback { get; private set; } = true;
	[Export]
	public int HapticStrength { get; private set; } = 50;
	[Export]
	public bool QuicktimeAutocomplete { get; private set; } = false;
	[Export]
	public bool AutoAim { get; private set; } = false;
	[Export]
	public int ColorblindMode { get; private set; } = 0;
	[Export]
	public bool DyslexiaMode { get; private set; } = false;

	//
	// gameplay
	//
	[Export]
	public bool EquipWeaponOnPickup { get; private set; } = true;
	[Export]
	public bool DrawAimLine { get; private set; } = true;
	[Export]
	public bool Hellbreaker { get; private set; } = false;
	[Export]
	public bool HellbreakerRevanents { get; private set; } = false;
	[Export]
	public bool CleanAudio { get; private set; } = false;

	[Export]
	public bool NetworkingEnabled { get; private set; } = true;
	[Export]
	public bool FriendsOnly { get; private set; } = false;
};