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
	MSAA_8x
};

public partial class SettingsData : Control {
	private Resource Default;

	private static readonly int MusicBus = AudioServer.GetBusIndex( "Music" );
	private static readonly int SfxBus = AudioServer.GetBusIndex( "SFX" );
	private static readonly int AmbienceBus = AudioServer.GetBusIndex( "Ambience" );

	private static WindowMode WindowMode;
	private static bool VSync;
	private static AntiAliasing AntiAliasing;
	private static RenderingServer.ShadowQuality ShadowQuality;
	private static int MaxFps;

	private static float HapticStrength;
	private static bool HapticEnabled;
	private static bool QuicktimeAutocomplete;
	private static int ColorblindMode;
	private static bool AutoAimEnabled;
	private static bool DyslexiaMode;
	
	private static bool EffectsOn;
	private static float EffectsVolume;
	private static bool MusicOn;
	private static float MusicVolume;
	private static bool MuteUnfocused;

	private static bool EquipWeaponOnPickup;
	private static bool HellbreakerEnabled;
	private static bool HellbreakerRevanents;

	private static Godot.Collections.Array<Resource> MappingContexts;
	private static RefCounted Remapper;
	private static Resource RemappingConfig;
	private static Godot.Collections.Array<RefCounted> RemappableItems;
	private static RefCounted MappingFormatter;

	public static RefCounted GetRemapper() {
		return Remapper;
	}
	public static RefCounted GetMappingFormatter() {
		return MappingFormatter;
	}

	public static WindowMode GetWindowMode() {
		return WindowMode;
	}
	public static void SetWindowMode( WindowMode mode ) {
		WindowMode = mode;
	}
	public static bool GetVSync() {
		return VSync;
	}
	public static void SetVSync( bool bVSync ) {
		VSync = bVSync;
	}
	public static AntiAliasing GetAntiAliasing() {
		return AntiAliasing;
	}
	public static void SetAntiAliasing( AntiAliasing mode ) {
		AntiAliasing = mode;
	}
	public static int GetMaxFps() {
		return MaxFps;
	}
	public static void SetMaxFps( int nMaxFps ) {
		MaxFps = nMaxFps;
	}

	public static bool GetEffectsOn() {
		return EffectsOn;
	}
	public static void SetEffectsOn( bool bEffectsOn ) {
		EffectsOn = bEffectsOn;
	}
	public static float GetEffectsVolume() {
		return EffectsVolume;
	}
	public static void SetEffectsVolume( float fEffectsVolume ) {
		EffectsVolume = fEffectsVolume;
	}
	public static bool GetMusicOn() {
		return MusicOn;
	}
	public static void SetMusicOn( bool bMusicOn ) {
		MusicOn = bMusicOn;
	}
	public static float GetMusicVolume() {
		return MusicVolume;
	}
	public static void SetMusicVolume( float fMusicVolume ) {
		MusicVolume = fMusicVolume;
	}
	public static bool GetMuteUnfocused() {
		return MuteUnfocused;
	}
	public static void SetMuteUnfocused( bool bMuteUnfocused ) {
		MuteUnfocused = bMuteUnfocused;
	}

	public static bool GetHapticEnabled() {
		return HapticEnabled;
	}
	public static void SetHapticEnabled( bool bHapticEnabled ) {
		HapticEnabled = bHapticEnabled;
	}
	public static float GetHapticStrength() {
		return HapticStrength;
	}
	public static void SetHapticStrength( float fHapticStrength ) {
		HapticStrength = fHapticStrength;
	}
	public static bool GetAutoAimEnabled() {
		return AutoAimEnabled;
	}
	public static void SetAutoAimEnabled( bool bAutoAimEnabled ) {
		AutoAimEnabled = bAutoAimEnabled;
	}
	public static bool GetDyslexiaMode() {
		return DyslexiaMode;
	}
	public static void SetDyslexiaMode( bool bDyslexiaMode ) {
		DyslexiaMode = bDyslexiaMode;
	}

	public static bool GetEquipWeaponOnPickup() {
		return EquipWeaponOnPickup;
	}
	public static void SetEquipWeaponOnPickup( bool bEquipWeapon ) {
		EquipWeaponOnPickup = bEquipWeapon;
	}

	private static void LoadAudioSettings( System.IO.BinaryReader reader ) {
		EffectsOn = reader.ReadBoolean();
		EffectsVolume = (float)reader.ReadDouble();
		MusicOn = reader.ReadBoolean();
		MusicVolume = (float)reader.ReadDouble();

		AudioServer.SetBusVolumeDb( MusicBus, MusicVolume / 100.0f );
		AudioServer.SetBusVolumeDb( SfxBus, EffectsVolume / 100.0f );

		AudioServer.SetBusMute( MusicBus, !MusicOn );
		AudioServer.SetBusMute( SfxBus, !EffectsOn );
	}
	private static void SaveAudioSettings( System.IO.BinaryWriter writer ) {
		writer.Write( EffectsOn );
		writer.Write( (double)EffectsVolume );
		writer.Write( MusicOn );
		writer.Write( (double)MusicVolume );
	}
	private static void LoadVideoSettings( System.IO.BinaryReader reader ) {
		WindowMode = (WindowMode)reader.ReadUInt32();
		MaxFps = reader.ReadInt32();
		ShadowQuality = (RenderingServer.ShadowQuality)reader.ReadInt64();
		AntiAliasing = (AntiAliasing)reader.ReadUInt32();
		VSync = reader.ReadBoolean();
	}
	private static void SaveVideoSettings( System.IO.BinaryWriter writer ) {
		writer.Write( (uint)WindowMode );
		writer.Write( MaxFps );
		writer.Write( (long)ShadowQuality );
		writer.Write( (uint)AntiAliasing );
		writer.Write( VSync );
	}
	private static void LoadAccessibilitySettings( System.IO.BinaryReader reader ) {
		ColorblindMode = reader.ReadInt32();
		HapticStrength = (float)reader.ReadDouble();
		HapticEnabled = reader.ReadBoolean();
		AutoAimEnabled = reader.ReadBoolean();
		DyslexiaMode = reader.ReadBoolean();
	}
	private static void SaveAccessibilitySettings( System.IO.BinaryWriter writer ) {
		writer.Write( ColorblindMode );
		writer.Write( (double)HapticStrength );
		writer.Write( HapticEnabled );
		writer.Write( AutoAimEnabled );
		writer.Write( DyslexiaMode );
	}
	private static void LoadGameplaySettings( System.IO.BinaryReader reader ) {
		EquipWeaponOnPickup = reader.ReadBoolean();
		HellbreakerEnabled = reader.ReadBoolean();
		HellbreakerRevanents = reader.ReadBoolean();
	}
	private static void SaveGameplaySettings( System.IO.BinaryWriter writer ) {
		writer.Write( EquipWeaponOnPickup );
		writer.Write( HellbreakerEnabled );
		writer.Write( HellbreakerRevanents );
	}

	private void GetDefaultConfig() {
		Default = ResourceLoader.Load( "res://resources/DefaultSettings.tres" );

		WindowMode = (WindowMode)(uint)Default.Get( "_window_mode" );
		VSync = (bool)Default.Get( "_vsync" );
		AntiAliasing = (AntiAliasing)(uint)Default.Get( "_anti_aliasing" );
		ShadowQuality = (RenderingServer.ShadowQuality)(long)Default.Get( "_shadow_quality" );
		MaxFps = (int)Default.Get( "_max_fps" );

		HapticStrength = (float)Default.Get( "_haptic_strength" );
		HapticEnabled = (bool)Default.Get( "_haptic_enabled" );
		QuicktimeAutocomplete = (bool)Default.Get( "_quicktime_autocomplete" );
		ColorblindMode = (int)Default.Get( "_colorblind_mode" );
		AutoAimEnabled = (bool)Default.Get( "_autoaim" );
		DyslexiaMode = (bool)Default.Get( "_dyslexia_mode" );

		EffectsOn = (bool)Default.Get( "_effects_on" );
		EffectsVolume = (float)Default.Get( "_effects_volume" );
		MusicOn = (bool)Default.Get( "_music_on" );
		MusicVolume = (float)Default.Get( "_music_volume" );
		MuteUnfocused = (bool)Default.Get( "_mute_unfocused" );

		EquipWeaponOnPickup = (bool)Default.Get( "_equip_weapon_on_pickup" );
		HellbreakerEnabled = (bool)Default.Get( "_hellbreaker" );
		HellbreakerRevanents = (bool)Default.Get( "_hellbreaker_revanents" );
	}

	public override void _Ready() {
		base._Ready();

		GetNode( "/root/Console" ).Call( "print_line", "Loading game configuration..." );

		RefCounted bindingSetup = (RefCounted)ResourceLoader.Load<GDScript>( "res://scripts/menus/settings_bindings.gd" ).New();

		Remapper = (RefCounted)bindingSetup.Get( "_remapper" );
		RemappingConfig = (Resource)bindingSetup.Get( "_remapping_config" );
		RemappableItems = (Godot.Collections.Array<RefCounted>)bindingSetup.Get( "_remappable_items" );
		MappingFormatter = (RefCounted)bindingSetup.Get( "_mapping_formatter" );

		GetDefaultConfig();

		string path = ProjectSettings.GlobalizePath( "user://settings.dat" );
		System.IO.FileStream stream;
		try {
			stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
		} catch ( System.IO.FileNotFoundException ) {
			GetNode( "/root/Console" ).Call( "print_line", "...settings file doesn't exist, using defaults" );
			return;
		}

		System.IO.BinaryReader reader = new System.IO.BinaryReader( stream );
		LoadVideoSettings( reader );
		LoadAudioSettings( reader );
		LoadAccessibilitySettings( reader );
		LoadGameplaySettings( reader );
	}

	public static void Save() {
		string path = ProjectSettings.GlobalizePath( "user://settings.dat" );
		System.IO.FileStream stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		System.IO.BinaryWriter writer = new System.IO.BinaryWriter( stream );

		SaveVideoSettings( writer );
		SaveAudioSettings( writer );
		SaveAccessibilitySettings( writer );
		SaveGameplaySettings( writer );

		ResourceSaver.Save( RemappingConfig, "user://input_context.tres" );
	}
};