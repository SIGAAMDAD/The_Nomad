/*
===========================================================================
The Nomad AGPL Source Code
Copyright (C) 2025 Noah Van Til

The Nomad Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

The Nomad Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad Source Code.  If not, see <http://www.gnu.org/licenses/>.

If you have questions concerning this license or the applicable additional
terms, you may contact me via email at nyvantil@gmail.com.
===========================================================================
*/

using Godot;

public enum WindowMode : byte {
	Windowed,
	BorderlessWindowed,
	Fullscreen,
	BorderlessFullscreen,
	ExclusiveFullscreen,

	Count
};

public enum WindowResolution : byte {
	Res_640x480,
	Res_800x600,
	Res_1024x768,
	Res_1280x720,
	Res_1280x768,
	Res_1280x800,
	Res_1280x1024,
	Res_1360x768,
	Res_1366x768,
	Res_1440x900,
	Res_1536x864,
	Res_1600x900,
	Res_1600x1200,
	Res_1680x1050,
	Res_1920x1080,
	Res_1920x1200,
	Res_2048x1152,
	Res_2048x1536,
	Res_2560x1080,
	Res_2560x1440,
	Res_2560x1600,
	Res_3440x1440,
	Res_3840x2160,

	Count
};

public enum AspectRatio : byte {
	Aspect_Automatic,
	Aspect_4_3,
	Aspect_16_10,
	Aspect_16_9,
	Aspect_21_9
};

public enum AntiAliasing : byte {
	None,
	FXAA,
	MSAA_2x,
	MSAA_4x,
	MSAA_8x,
	TAA,
	FXAA_and_TAA
};

public enum ShadowQuality : byte {
	Off,
	Low,
	Medium,
	High,
	Ultra
};

public enum ShadowFilterQuality : byte {
	Off,
	Low,
	High
};

public enum AnimationQuality : byte {
	Low,
	Medium,
	High
};

public enum LightingQuality : byte {
	VeryLow,
	Low,
	High
};

public enum ParticleQuality : byte {
	Low,
	High
};

public enum VSyncMode : byte {
	Off,
	Adaptive,
	On,
	TripleBuffered
};

public enum DRSPreset : byte {
	UltraPerformance,
	Performance,
	Balanced,
	Quality,
	UltraQuality,

	Dynamic // trying to hit a target framerate
};

public enum AutoAimMode : byte {
	Off,
	Soft,
	LockOn
};

public enum HUDPreset : byte {
	// all hud elements are used
	Full,

	// only essential hud elements are presented
	Partial,
	
	// all hud ui elements are hidden
	Expert
};

public enum PerformanceOverlayPreset : byte {
	Hidden,
	FpsOnly,
	Partial,
	Full
};

public enum ColorblindMode : byte {
	None,
	Protanopia,
	Deutanopia,
	Trianopia,
	Monochromacy
};

/*
===================================================================================

DefaultSettings

defines default configuration values

===================================================================================
*/

public partial class DefaultSettings : Resource {
	//
	// video
	//
	[Export] public WindowMode WindowMode { get; private set; } = WindowMode.Fullscreen;
	[Export] public WindowResolution Resolution { get; private set; } = WindowResolution.Res_640x480;
	[Export] public AspectRatio AspectRatio { get; private set; } = AspectRatio.Aspect_Automatic;
	[Export] public DRSPreset DRSPreset { get; private set; } = DRSPreset.Balanced;
	[Export] public int DRSTargetFrames { get; private set; } = 60;
	[Export] public VSyncMode Vsync { get; private set; } = VSyncMode.Off;
	[Export] public AntiAliasing AntiAliasing { get; private set; } = AntiAliasing.None;
	[Export] public ShadowQuality ShadowQuality { get; private set; } = ShadowQuality.Medium;
	[Export] public ShadowFilterQuality ShadowFilterQuality { get; private set; } = ShadowFilterQuality.Low;
	[Export] public ParticleQuality ParticleQuality { get; private set; } = ParticleQuality.Low;
	[Export] public AnimationQuality AnimationQuality { get; private set; } = AnimationQuality.Low;
	[Export] public LightingQuality LightingQuality { get; private set; } = LightingQuality.Low;
	[Export] public int MaxFps { get; private set; } = 60;
	[Export] public bool BloomEnabled { get; private set; } = true;
	[Export] public PerformanceOverlayPreset PerformanceStats { get; private set; } = PerformanceOverlayPreset.Hidden;
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
	[Export] public AutoAimMode AutoAim { get; private set; } = AutoAimMode.Off;
	[Export] public bool DyslexiaMode { get; private set; } = false;
	[Export] public bool EnableTutorials { get; private set; } = false;
	[Export] public ColorblindMode ColorblindMode { get; private set; } = ColorblindMode.None;
	[Export] public bool TextToSpeech { get; private set; } = false;
	[Export] public int TtsVoiceIndex { get; private set; } = 0;
	[Export] public float UIScale { get; private set; } = 1.0f;
	[Export] public HUDPreset HUDPreset { get; private set; } = HUDPreset.Full;
	[Export] public bool DrawAimLine { get; private set; } = true;

	//
	// gameplay
	//
	[Export] public bool EquipWeaponOnPickup { get; private set; } = true;
	[Export] public bool Hellbreaker { get; private set; } = false;
	[Export] public bool HellbreakerRevanents { get; private set; } = false;
	[Export] public bool StopGameOnFocusLost { get; private set; } = true;

	//
	// networking
	//
	[Export] public bool NetworkingEnabled { get; private set; } = true;
	[Export] public bool BountyHuntEnabled { get; private set; } = true;
	[Export] public bool CODLobbies { get; private set; } = false;
};