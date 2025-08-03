using Godot;

public enum WindowMode : uint {
	Windowed,
	BorderlessWindowed,
	Fullscreen,
	BorderlessFullscreen,
	ExclusiveFullscreen
};

public enum Resolution : uint {
	Res_640x480,
	Res_800x600,
	Res_1280x768,
	Res_1920x1080,
	Res_1600x900
};

public enum AspectRatio : uint {
	Aspect_Automatic,
	Aspect_4_3,
	Aspect_16_10,
	Aspect_16_9,
	Aspect_21_9
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
	Low,
	Medium,
	High,
	Ultra
};

public enum ShadowFilterQuality : uint {
	Off,
	Low,
	High
};

public enum ParticleQuality : uint {
	Low,
	High
};

public enum VSyncMode : uint {
	Off,
	Adaptive,
	On,
	TripleBuffered
};

public enum DRSPreset : uint {
	UltraPerformance,
	Performance,
	Balanced,
	Quality,
	UltraQuality,

	Dynamic // trying to hit a target framerate
};

public partial class DefaultSettings : Resource {
	//
	// video
	//
	[Export] public WindowMode WindowMode { get; private set; } = WindowMode.Fullscreen;
	[Export] public Resolution Resolution { get; private set; } = Resolution.Res_640x480;
	[Export] public AspectRatio AspectRatio { get; private set; } = AspectRatio.Aspect_Automatic;
	[Export] public DRSPreset DRSPreset { get; private set; } = DRSPreset.Balanced;
	[Export] public int DRSTargetFrames { get; private set; } = 60;
	[Export] public bool Vsync { get; private set; } = false;
	[Export] public AntiAliasing AntiAliasing { get; private set; } = AntiAliasing.None;
	[Export] public ShadowQuality ShadowQuality { get; private set; } = ShadowQuality.Medium;
	[Export] public ShadowFilterQuality ShadowFilterQuality { get; private set; } = ShadowFilterQuality.Low;
	[Export] public ParticleQuality ParticleQuality { get; private set; } = ParticleQuality.Low;
	[Export] public int MaxFps { get; private set; } = 60;
	[Export] public bool BloomEnabled { get; private set; } = true;
	[Export] public bool ShowFps { get; private set; } = false;
	[Export] public bool ShowBlood { get; private set; } = true;

	//
	// audio
	//
	[Export] public bool SoundEffectsOn { get; private set; } = true;
	[Export] public float SoundEffectsVolume { get; private set; } = 50.0f;
	[Export] public bool MusicOn { get; private set; } = true;
	[Export] public float MusicVolume { get; private set; } = 50.0f;

	//
	// accessibility
	//
	[Export] public bool HapticFeedback { get; private set; } = true;
	[Export] public int HapticStrength { get; private set; } = 50;
	[Export] public bool QuicktimeAutocomplete { get; private set; } = false;
	[Export] public bool AutoAim { get; private set; } = false;
	[Export] public int ColorblindMode { get; private set; } = 0;
	[Export] public bool DyslexiaMode { get; private set; } = false;
	[Export] public bool EnableTutorials { get; private set; } = false;
	[Export] public float UIScale { get; private set; } = 1.0f;

	//
	// gameplay
	//
	[Export] public bool EquipWeaponOnPickup { get; private set; } = true;
	[Export] public bool DrawAimLine { get; private set; } = true;
	[Export] public bool Hellbreaker { get; private set; } = false;
	[Export] public bool HellbreakerRevanents { get; private set; } = false;
	[Export] public bool ExpertUI { get; private set; } = false;
	[Export] public bool StopGameOnFocusLost { get; private set; } = true;

	//
	// networking
	//
	[Export] public bool NetworkingEnabled { get; private set; } = true;
	[Export] public bool FriendsOnly { get; private set; } = false;
	[Export] public bool CODLobbies { get; private set; } = false;
};