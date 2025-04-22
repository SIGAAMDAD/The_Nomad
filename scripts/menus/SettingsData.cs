using Microsoft.Extensions.Configuration.Ini;
using Godot;
using System.Collections.Generic;
using System;

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

public partial class SettingsData : Control {
	private Resource Default;

	private static WindowMode WindowMode;
	private static DisplayServer.VSyncMode VSyncMode;
	private static AntiAliasing AntiAliasing;
	private static int MaxFps;
	private static bool BloomEnabled;
	private static bool SunLightEnabled;
	private static ShadowQuality SunShadowQuality;
	private static bool ShowFPS;

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

	private static bool EnableNetworking;
	private static bool FriendsOnlyNetworking;

	private static float EffectsVolumeDb_Flat = 0.0f;
	private static float MusicVolumeDb_Flat = 0.0f;

	private static Godot.Collections.Array<Resource> MappingContexts;
	private static RefCounted Remapper;
	private static Resource RemappingConfig;
	private static Godot.Collections.Array<RefCounted> RemappableItems;
	private static RefCounted MappingFormatter;

	public static SettingsData Instance = null;

	[Signal]
	public delegate void EffectsToggledEventHandler();
	[Signal]
	public delegate void EffectsVolumeChangedEventHandler();
	[Signal]
	public delegate void MusicToggledEventHandler();
	[Signal]
	public delegate void MusicVolumeChangedEventHandler();

	public static RefCounted GetRemapper() => Remapper;
	public static RefCounted GetMappingFormatter() => MappingFormatter;

	public static bool GetNetworkingEnabled() => EnableNetworking;
	public static void SetNetworkingEnabled( bool bNetworking ) => EnableNetworking = bNetworking;
	public static bool GetFriendsOnlyNetworking() => FriendsOnlyNetworking;
	public static void SetFriendsOnlyNetworking( bool bFriendsOnlyNetworking ) => FriendsOnlyNetworking = bFriendsOnlyNetworking;
	
	public static WindowMode GetWindowMode() => WindowMode;
	public static void SetWindowMode( WindowMode mode ) => WindowMode = mode;
	public static DisplayServer.VSyncMode GetVSync() => VSyncMode;
	public static void SetVSync( DisplayServer.VSyncMode vsync ) => VSyncMode = vsync;
	public static AntiAliasing GetAntiAliasing() => AntiAliasing;
	public static void SetAntiAliasing( AntiAliasing mode ) => AntiAliasing = mode;
	public static bool GetBloomEnabled() => BloomEnabled;
	public static void SetBloomEnabled( bool bBloomEnabled ) => BloomEnabled = bBloomEnabled;
	public static bool GetSunLightEnabled() => SunLightEnabled;
	public static void SetSunLightEnabled( bool bSunLightEnabled ) => SunLightEnabled = bSunLightEnabled;
	public static ShadowQuality GetSunShadowQuality() => SunShadowQuality;
	public static void SetSunShadowQuality( ShadowQuality sunShadowQuality ) => SunShadowQuality = sunShadowQuality;
	public static bool GetShowFPS() => ShowFPS;
	public static void SetShowFPS( bool bShowFPS ) {
		ShowFPS = bShowFPS;
		Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ShowFPS ? ProcessModeEnum.Always : ProcessModeEnum.Disabled;
		Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = ShowFPS;
	}

	public static bool GetEffectsOn() => EffectsOn;
	public static void SetEffectsOn( bool bEffectsOn ) {
		EffectsOn = bEffectsOn;
		Instance.EmitSignalEffectsToggled();
	}
	public static float GetEffectsVolume() => EffectsVolume;
	public static float GetEffectsVolumeLinear() => EffectsVolumeDb_Flat;
	public static void SetEffectsVolume( float fEffectsVolume ) {
		EffectsVolume = fEffectsVolume;
		EffectsVolumeDb_Flat = Mathf.LinearToDb( EffectsVolume * 0.01f );
		Instance.EmitSignalEffectsVolumeChanged();
	}
	public static bool GetMusicOn() => MusicOn;
	public static void SetMusicOn( bool bMusicOn ) {
		MusicOn = bMusicOn;
		Instance.EmitSignalMusicToggled();
	}
	public static float GetMusicVolume() => MusicVolume;
	public static float GetMusicVolumeLinear() => MusicVolumeDb_Flat;
	public static void SetMusicVolume( float fMusicVolume ) {
		MusicVolume = fMusicVolume;
		MusicVolumeDb_Flat = Mathf.LinearToDb( MusicVolume * 0.01f );
		Instance.EmitSignalMusicVolumeChanged();
	}
	public static bool GetMuteUnfocused() => MuteUnfocused;
	public static void SetMuteUnfocused( bool bMuteUnfocused ) => MuteUnfocused = bMuteUnfocused;

	public static bool GetHapticEnabled() => HapticEnabled;
	public static void SetHapticEnabled( bool bHapticEnabled ) => HapticEnabled = bHapticEnabled;
	public static float GetHapticStrength() => HapticStrength;
	public static void SetHapticStrength( float fHapticStrength ) => HapticStrength = fHapticStrength;
	public static bool GetAutoAimEnabled() => AutoAimEnabled;
	public static void SetAutoAimEnabled( bool bAutoAimEnabled ) => AutoAimEnabled = bAutoAimEnabled;
	public static bool GetDyslexiaMode() => DyslexiaMode;
	public static void SetDyslexiaMode( bool bDyslexiaMode ) => DyslexiaMode = bDyslexiaMode;

	public static bool GetEquipWeaponOnPickup() => EquipWeaponOnPickup;
	public static void SetEquipWeaponOnPickup( bool bEquipWeapon ) => EquipWeaponOnPickup = bEquipWeapon;

	private static void LoadAudioSettings( IDictionary<string, string> config ) {
		EffectsOn = Convert.ToBoolean( config[ "Audio:SFXEnabled" ] );
		SetEffectsVolume( (float)Convert.ToDouble( config[ "Audio:SFXVolume" ] ) );
		MusicOn = Convert.ToBoolean( config[ "Audio:MusicEnabled" ] );
		SetMusicVolume( (float)Convert.ToDouble( config[ "Audio:MusicVolume" ] ) );
	}
	private static void SaveAudioSettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Audio]" );
		writer.WriteLine( string.Format( "SFXEnabled={0}", EffectsOn.ToString() ) );
		writer.WriteLine( string.Format( "SFXVolume={0}", EffectsVolume ) );
		writer.WriteLine( string.Format( "MusicEnabled={0}", MusicOn.ToString() ) );
		writer.WriteLine( string.Format( "MusicVolume={0}", MusicVolume ) );
		writer.WriteLine();
	}
	private static void LoadVideoSettings( IDictionary<string, string> config ) {
		switch ( config[ "Video:WindowMode" ] ) {
		case "Windowed":
			WindowMode = WindowMode.Windowed;
			break;
		case "BorderlessWindowed":
			WindowMode = WindowMode.BorderlessWindowed;
			break;
		case "Fullscreen":
			WindowMode = WindowMode.Fullscreen;
			break;
		case "BorderlessFullscreen":
			WindowMode = WindowMode.BorderlessFullscreen;
			break;
		default:
			Console.PrintError( "Unknown window mode \"" + config[ "Video:WindowMode" ] + " defaulting to fullscreen." );
			WindowMode = WindowMode.Fullscreen;
			break;
		};
		MaxFps = Convert.ToInt32( config[ "Video:MaxFps" ] );
//		ShadowQuality = (RenderingServer.ShadowQuality)Convert.ToInt64( config[ "Video:ShadowQuality" ] );
		switch ( config[ "Video:AntiAliasing" ] ) {
		case "None":
			AntiAliasing = AntiAliasing.None;
			break;
		case "FXAA":
			AntiAliasing = AntiAliasing.FXAA;
			break;
		case "MSAA_2x":
			AntiAliasing = AntiAliasing.MSAA_2x;
			break;
		case "MSAA_4x":
			AntiAliasing = AntiAliasing.MSAA_4x;
			break;
		case "MSAA_8x":
			AntiAliasing = AntiAliasing.MSAA_8x;
			break;
		case "TAA":
			AntiAliasing = AntiAliasing.TAA;
			break;
		case "FXAA_and_TAA":
			AntiAliasing = AntiAliasing.FXAA_and_TAA;
			break;
		};
		switch ( config[ "Video:SunShadowQuality" ] ) {
		case "None":
			SunShadowQuality = ShadowQuality.Off;
			break;
		case "NoFilter":
			SunShadowQuality = ShadowQuality.NoFilter;
			break;
		case "Low":
			SunShadowQuality = ShadowQuality.Low;
			break;
		case "High":
			SunShadowQuality = ShadowQuality.High;
			break;
		};
		switch ( config[ "Video:VSync" ] ) {
		case "Disabled":
			VSyncMode = DisplayServer.VSyncMode.Disabled;
			break;
		case "Adaptive":
			VSyncMode = DisplayServer.VSyncMode.Adaptive;
			break;
		case "Enabled":
			VSyncMode = DisplayServer.VSyncMode.Enabled;
			break;
		};
		BloomEnabled = Convert.ToBoolean( config[ "Video:Bloom" ] );
		SunLightEnabled = Convert.ToBoolean( config[ "Video:SunLight" ] );
		ShowFPS = Convert.ToBoolean( config[ "Video:ShowFPS" ] );
	}
	private static void SaveVideoSettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Video]" );
		writer.WriteLine( string.Format( "WindowMode={0}", WindowMode ) );
		writer.WriteLine( string.Format( "MaxFps={0}", MaxFps ) );
//		writer.WriteLine( string.Format( "ShadowQuality={0}", ShadowQuality ) );
		writer.WriteLine( string.Format( "AntiAliasing={0}", AntiAliasing ) );
		writer.WriteLine( string.Format( "VSync={0}", VSyncMode ) );
		writer.WriteLine( string.Format( "Bloom={0}", BloomEnabled.ToString() ) );
		writer.WriteLine( string.Format( "SunShadowQuality={0}", SunShadowQuality ) );
		writer.WriteLine( string.Format( "SunLight={0}", SunLightEnabled ) );
		writer.WriteLine( string.Format( "ShowFPS={0}", ShowFPS.ToString() ) );
		writer.WriteLine();
	}
	private static void LoadAccessibilitySettings( IDictionary<string, string> config ) {
		ColorblindMode = Convert.ToInt32( config[ "Accessibility:ColorblindMode" ] );
		HapticStrength = (float)Convert.ToDouble( config[ "Accessibility:HapticStrength" ] );
		HapticEnabled = Convert.ToBoolean( config[ "Accessibility:HapticEnabled" ] );
		AutoAimEnabled = Convert.ToBoolean( config[ "Accessibility:AutoAimEnabled" ] );
		DyslexiaMode = Convert.ToBoolean( config[ "Accessibility:DyslexiaMode" ] );
		QuicktimeAutocomplete = Convert.ToBoolean( config[ "Accessibility:QuicktimeAutocomplete" ] );
	}
	private static void SaveAccessibilitySettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Accessibility]" );
		writer.WriteLine( string.Format( "ColorblindMode={0}", ColorblindMode ) );
		writer.WriteLine( string.Format( "HapticStrength={0}", HapticStrength ) );
		writer.WriteLine( string.Format( "HapticEnabled={0}", HapticEnabled.ToString() ) );
		writer.WriteLine( string.Format( "AutoAimEnabled={0}", AutoAimEnabled.ToString() ) );
		writer.WriteLine( string.Format( "DyslexiaMode={0}", DyslexiaMode.ToString() ) );
		writer.WriteLine( string.Format( "QuicktimeAutocomplete={0}", QuicktimeAutocomplete.ToString() ) );
		writer.WriteLine();
	}
	private static void LoadGameplaySettings( IDictionary<string, string> config ) {
		EquipWeaponOnPickup = Convert.ToBoolean( config[ "Gameplay:EquipWeaponOnPickup" ] );
		HellbreakerEnabled = Convert.ToBoolean( config[ "Gameplay:HellbreakerEnabled" ] );
		HellbreakerRevanents = Convert.ToBoolean( config[ "Gameplay:HellbreakerRevanents" ] );
	}
	private static void SaveGameplaySettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Gameplay]" );
		writer.WriteLine( string.Format( "EquipWeaponOnPickup={0}", EquipWeaponOnPickup.ToString() ) );
		writer.WriteLine( string.Format( "HellbreakerEnabled={0}", HellbreakerEnabled.ToString() ) );
		writer.WriteLine( string.Format( "HellbreakerRevanents={0}", HellbreakerRevanents.ToString() ) );
		writer.WriteLine();
	}
	private static void LoadNetworkingSettings( IDictionary<string, string> config ) {
		EnableNetworking = Convert.ToBoolean( config[ "Networking:EnableNetworking" ] );
		FriendsOnlyNetworking = Convert.ToBoolean( config[ "Networking:FriendsOnlyNetworking" ] );
	}
	private static void SaveNetworkingSettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Networking]" );
		writer.WriteLine( string.Format( "EnableNetworking={0}", EnableNetworking.ToString() ) );
		writer.WriteLine( string.Format( "FriendsOnlyNetworking={0}", FriendsOnlyNetworking.ToString() ) );
		writer.WriteLine();
	}

	private void GetDefaultConfig() {
		Default = ResourceLoader.Load( "res://resources/DefaultSettings.tres" );

		WindowMode = (WindowMode)(uint)Default.Get( "_window_mode" );
		VSyncMode = (DisplayServer.VSyncMode)(uint)Default.Get( "_vsync_mode" );
		AntiAliasing = (AntiAliasing)(uint)Default.Get( "_anti_aliasing" );
		MaxFps = (int)Default.Get( "_max_fps" );
		BloomEnabled = (bool)Default.Get( "_bloom_enabled" );
		SunShadowQuality = (ShadowQuality)(uint)Default.Get( "_sun_shadow_quality" );
		SunLightEnabled = (bool)Default.Get( "_sun_light_enabled" );
		SetShowFPS( (bool)Default.Get( "_show_fps" ) );

		HapticStrength = (float)Default.Get( "_haptic_strength" );
		HapticEnabled = (bool)Default.Get( "_haptic_feedback" );
		QuicktimeAutocomplete = (bool)Default.Get( "_quicktime_autocomplete" );
		ColorblindMode = (int)Default.Get( "_colorblind_mode" );
		AutoAimEnabled = (bool)Default.Get( "_autoaim" );
		DyslexiaMode = (bool)Default.Get( "_dyslexia_mode" );

		EffectsOn = (bool)Default.Get( "_sound_effects_on" );
		SetEffectsVolume( (float)Default.Get( "_sound_effects_volume" ) );
		MusicOn = (bool)Default.Get( "_music_on" );
		SetMusicVolume( (float)Default.Get( "_music_volume" ) );
		MuteUnfocused = (bool)Default.Get( "_mute_unfocused" );

		EquipWeaponOnPickup = (bool)Default.Get( "_equip_weapon_on_pickup" );
		HellbreakerEnabled = (bool)Default.Get( "_hellbreaker" );
		HellbreakerRevanents = (bool)Default.Get( "_hellbreaker_revanents" );

		EnableNetworking = (bool)Default.Get( "_networking_enabled" );
		FriendsOnlyNetworking = (bool)Default.Get( "_friends_only" );
	}

	public override void _Ready() {
		base._Ready();

		Instance = this;

		Console.PrintLine( "Loading game configuration..." );

		RefCounted bindingSetup = (RefCounted)ResourceLoader.Load<GDScript>( "res://scripts/menus/settings_bindings.gd" ).New();

		Remapper = (RefCounted)bindingSetup.Get( "_remapper" );
		RemappingConfig = (Resource)bindingSetup.Get( "_remapping_config" );
		RemappableItems = (Godot.Collections.Array<RefCounted>)bindingSetup.Get( "_remappable_items" );
		MappingFormatter = (RefCounted)bindingSetup.Get( "_mapping_formatter" );

		GetDefaultConfig();

		string path = ProjectSettings.GlobalizePath( "user://settings.ini" );
		System.IO.FileStream stream;

		try {
			stream = new System.IO.FileStream( path, System.IO.FileMode.Open );
		} catch ( System.IO.FileNotFoundException ) {
			Console.PrintLine( "...settings file doesn't exist, using defaults" );
			return;
		}

		IDictionary<string, string> iniData = IniStreamConfigurationProvider.Read( stream );
		LoadVideoSettings( iniData );
		LoadAudioSettings( iniData );
		LoadAccessibilitySettings( iniData );
		LoadGameplaySettings( iniData );
		LoadNetworkingSettings( iniData );

		Console.PrintLine( "...Finished applying settings" );
	}

	public static void Save() {
		Console.PrintLine( "Saving configuration data..." );

		string path = ProjectSettings.GlobalizePath( "user://settings.ini" );
		System.IO.FileStream stream = new System.IO.FileStream( path, System.IO.FileMode.Create );
		System.IO.StreamWriter writer = new System.IO.StreamWriter( stream );

		SaveVideoSettings( writer );
		SaveAudioSettings( writer );
		SaveAccessibilitySettings( writer );
		SaveGameplaySettings( writer );
		SaveNetworkingSettings( writer );
		writer.Flush();

		ResourceSaver.Save( RemappingConfig, "user://input_context.tres" );

		SteamManager.SaveCloudFile( "settings.ini" );
		SteamManager.SaveCloudFile( "input_context.tres" );
	}
};