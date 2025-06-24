using Microsoft.Extensions.Configuration.Ini;
using Godot;
using System.Collections.Generic;
using System;

public partial class SettingsData : Control {
	private DefaultSettings Default;

	//
	// video options
	//
	private static WindowMode WindowMode;
	private static DRSPreset DRSPreset;
	private static AspectRatio AspectRatio;
	private static Resolution Resolution;
	private static ShadowQuality ShadowQuality;
	private static ShadowFilterQuality ShadowFilterQuality;
	private static VSyncMode VSyncMode;
	private static AntiAliasing AntiAliasing;
	private static int MaxFps;
	private static int DRSTargetFrames;
	private static bool BloomEnabled;
	private static bool ShowFPS;
	private static bool ShowBlood;

	//
	// accessibility options
	//
	private static float HapticStrength;
	private static bool HapticEnabled;
	private static bool QuicktimeAutocomplete;
	private static int ColorblindMode;
	private static bool AutoAimEnabled;
	private static bool DyslexiaMode;
	private static bool EnableTutorials;
	private static SaveMode SaveMode;

	//
	// audio options
	//
	private static bool EffectsOn;
	private static float EffectsVolume;
	private static bool MusicOn;
	private static float MusicVolume;
	private static bool MuteUnfocused;

	//
	// gameplay options
	//
	private static bool EquipWeaponOnPickup;
	private static bool HellbreakerEnabled;
	private static bool HellbreakerRevanents;
	private static bool CleanAudio;

	//
	// network options
	//
	private static bool EnableNetworking;
	private static bool FriendsOnlyNetworking;

	private static float EffectsVolumeDb_Flat = 0.0f;
	private static float MusicVolumeDb_Flat = 0.0f;

	private static Godot.Collections.Array<Resource> MappingContexts;
	private static RefCounted Remapper;
	private static Resource RemappingConfig;
	private static Godot.Collections.Array<RefCounted> RemappableItems;
	private static RefCounted MappingFormatter;

	private static int LastSaveSlot = 0;

	public static SettingsData Instance = null;

	[Signal]
	public delegate void SettingsChangedEventHandler();

	public static RefCounted GetRemapper() => Remapper;
	public static RefCounted GetMappingFormatter() => MappingFormatter;

	public static bool GetNetworkingEnabled() => EnableNetworking;
	public static void SetNetworkingEnabled( bool bNetworking ) => EnableNetworking = bNetworking;
	public static bool GetFriendsOnlyNetworking() => FriendsOnlyNetworking;
	public static void SetFriendsOnlyNetworking( bool bFriendsOnlyNetworking ) => FriendsOnlyNetworking = bFriendsOnlyNetworking;

	public static WindowMode GetWindowMode() => WindowMode;
	public static void SetWindowMode( WindowMode mode ) => WindowMode = mode;
	public static DRSPreset GetDRSPreset() => DRSPreset;
	public static void SetDRSPreset( DRSPreset preset ) => DRSPreset = preset;
	public static int GetDRSTargetFrames() => DRSTargetFrames;
	public static void SetDRSTargetFrames( int nFrames ) => DRSTargetFrames = nFrames;
	public static Resolution GetResolution() => Resolution;
	public static void SetResolution( Resolution resolution ) => Resolution = resolution;
	public static int GetMaxFps() => MaxFps;
	public static void SetMaxFps( int nMaxFps ) {
		MaxFps = nMaxFps;
		Engine.MaxFps = MaxFps;
	}
	public static ShadowQuality GetShadowQuality() => ShadowQuality;
	public static void SetShadowQuality( ShadowQuality quality ) => ShadowQuality = quality;
	public static ShadowFilterQuality GetShadowFilterQuality() => ShadowFilterQuality;
	public static void SetShadowFilterQuality( ShadowFilterQuality quality ) => ShadowFilterQuality = quality;
	public static VSyncMode GetVSync() => VSyncMode;
	public static void SetVSync( VSyncMode vsync ) => VSyncMode = vsync;
	public static AntiAliasing GetAntiAliasing() => AntiAliasing;
	public static void SetAntiAliasing( AntiAliasing mode ) => AntiAliasing = mode;
	public static bool GetBloomEnabled() => BloomEnabled;
	public static void SetBloomEnabled( bool bBloomEnabled ) => BloomEnabled = bBloomEnabled;
	public static bool GetShowFPS() => ShowFPS;
	public static void SetShowFPS( bool bShowFPS ) {
		ShowFPS = bShowFPS;
		Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ShowFPS ? ProcessModeEnum.Always : ProcessModeEnum.Disabled;
		Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = ShowFPS;
	}
	public static bool GetShowBlood() => ShowBlood;
	public static void SetShowBlood( bool bShowBlood ) => ShowBlood = bShowBlood;

	public static bool GetEffectsOn() => EffectsOn;
	public static void SetEffectsOn( bool bEffectsOn ) => EffectsOn = bEffectsOn;
	public static float GetEffectsVolume() => EffectsVolume;
	public static float GetEffectsVolumeLinear() => EffectsVolumeDb_Flat;
	public static void SetEffectsVolume( float fEffectsVolume ) {
		EffectsVolume = fEffectsVolume;
		EffectsVolumeDb_Flat = Mathf.LinearToDb( EffectsVolume * 0.01f );
	}
	public static bool GetMusicOn() => MusicOn;
	public static void SetMusicOn( bool bMusicOn ) => MusicOn = bMusicOn;
	public static float GetMusicVolume() => MusicVolume;
	public static float GetMusicVolumeLinear() => MusicVolumeDb_Flat;
	public static void SetMusicVolume( float fMusicVolume ) {
		MusicVolume = fMusicVolume;
		MusicVolumeDb_Flat = Mathf.LinearToDb( MusicVolume * 0.01f );
	}
	public static bool GetMuteUnfocused() => MuteUnfocused;
	public static void SetMuteUnfocused( bool bMuteUnfocused ) => MuteUnfocused = bMuteUnfocused;

	public static bool GetHapticEnabled() => HapticEnabled;
	public static void SetHapticEnabled( bool bHapticEnabled ) => HapticEnabled = bHapticEnabled;
	public static float GetHapticStrength() => HapticStrength;
	public static void SetHapticStrength( float fHapticStrength ) => HapticStrength = fHapticStrength;
	public static bool GetAutoAimEnabled() => AutoAimEnabled;
	public static void SetAutoAimEnabled( bool bAutoAimEnabled ) => AutoAimEnabled = bAutoAimEnabled;
	public static bool GetTutorialsEnabled() => EnableTutorials;
	public static void SetTutorialsEnabled( bool bTutorialsEnabled ) => EnableTutorials = bTutorialsEnabled;
	public static bool GetDyslexiaMode() => DyslexiaMode;
	public static void SetDyslexiaMode( bool bDyslexiaMode ) => DyslexiaMode = bDyslexiaMode;

	public static bool GetEquipWeaponOnPickup() => EquipWeaponOnPickup;
	public static void SetEquipWeaponOnPickup( bool bEquipWeapon ) => EquipWeaponOnPickup = bEquipWeapon;
	public static bool GetHellbreakerEnabled() => HellbreakerEnabled;
	public static void SetHellbreakerEnabled( bool bHellbreakerEnabled ) => HellbreakerEnabled = bHellbreakerEnabled;
	public static bool GetCleanAudio() => CleanAudio;
	public static void SetCleanAudio( bool bCleanAudio ) => CleanAudio = bCleanAudio;

	public static int GetSaveSlot() => LastSaveSlot;
	public static void SetSaveSlot( int nSlot ) => LastSaveSlot = nSlot;

	private void UpdateWindowScale() {
		Godot.Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
		Godot.Vector2I windowSize = GetWindow().GetSizeWithDecorations();
		GetWindow().SetImePosition( centerScreen - windowSize / 2 );
	}
	private void ApplyVideoSettings() {
		switch ( VSyncMode ) {
		case VSyncMode.On:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Enabled );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				2
			);
			break;
		case VSyncMode.Off:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Disabled );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				2
			);
			break;
		case VSyncMode.Adaptive:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Adaptive );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				2
			);
			break;
		case VSyncMode.TripleBuffered:
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Mailbox );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				3
			);
			break;
		};

//		switch ( AspectRatio ) {
//		case AspectRatio.Aspect_Automatic:
//			ProjectSettings.SetSetting( "display/window/stretch/scale",  );
//			break;
//		};

		Godot.Vector2I windowSize = Godot.Vector2I.Zero;
		switch ( Resolution ) {
		case Resolution.Res_640x480:
			windowSize = new Godot.Vector2I( 640, 480 );
			break;
		case Resolution.Res_800x600:
			windowSize = new Godot.Vector2I( 800, 600 );
			break;
		case Resolution.Res_1280x768:
			windowSize = new Godot.Vector2I( 1280, 768 );
			break;
		case Resolution.Res_1920x1080:
			windowSize = new Godot.Vector2I( 1920, 1080 );
			break;
		case Resolution.Res_1600x900:
			windowSize = new Godot.Vector2I( 1600, 900 );
			break;
		default:
			windowSize = new Godot.Vector2I( 640, 480 );
			break;
		};
		GetWindow().Size = windowSize;

		Engine.MaxFps = MaxFps;

		Rid viewport = GetTree().Root.GetViewportRid();
		switch ( AntiAliasing ) {
		case AntiAliasing.None:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case AntiAliasing.FXAA:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Fxaa );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case AntiAliasing.MSAA_2x:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa2X );
			break;
		case AntiAliasing.MSAA_4x:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa4X );
			break;
		case AntiAliasing.MSAA_8x:
			RenderingServer.ViewportSetUseTaa( viewport, false );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Msaa8X );
			break;
		case AntiAliasing.TAA:
			RenderingServer.ViewportSetUseTaa( viewport, true );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Disabled );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		case AntiAliasing.FXAA_and_TAA:
			RenderingServer.ViewportSetUseTaa( viewport, true );
			RenderingServer.ViewportSetScreenSpaceAA( viewport, RenderingServer.ViewportScreenSpaceAA.Fxaa );
			RenderingServer.ViewportSetMsaa2D( viewport, RenderingServer.ViewportMsaa.Disabled );
			break;
		};

		switch ( WindowMode ) {
		case WindowMode.Windowed:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Windowed );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, false );
			break;
		case WindowMode.BorderlessWindowed:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Windowed );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			break;
		case WindowMode.Fullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Fullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, false );
			break;
		case WindowMode.BorderlessFullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.Fullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			break;
		case WindowMode.ExclusiveFullscreen:
			DisplayServer.WindowSetMode( DisplayServer.WindowMode.ExclusiveFullscreen );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, true );
			break;
		};

		UpdateWindowScale();
	}

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
		case "ExclusiveFullscreen":
			WindowMode = WindowMode.ExclusiveFullscreen;
			break;
		default:
			Console.PrintError( "Unknown window mode \"" + config[ "Video:WindowMode" ] + " defaulting to fullscreen." );
			WindowMode = WindowMode.Fullscreen;
			break;
		};
		switch ( config[ "Video:Resolution" ] ) {
		case "Res_640x480":
			Resolution = Resolution.Res_640x480;
			break;
		case "Res_800x600":
			Resolution = Resolution.Res_800x600;
			break;
		case "Res_1280x768":
			Resolution = Resolution.Res_1280x768;
			break;
		case "Res_1920x1080":
			Resolution = Resolution.Res_1920x1080;
			break;
		case "Res_1600x900":
			Resolution = Resolution.Res_1600x900;
			break;
		};
		switch ( config[ "Video:AspectRatio" ] ) {
		case "Aspect_Automatic":
			AspectRatio = AspectRatio.Aspect_Automatic;
			break;
		case "Aspect_4_3":
			AspectRatio = AspectRatio.Aspect_4_3;
			break;
		case "Aspect_16_10":
			AspectRatio = AspectRatio.Aspect_16_10;
			break;
		case "Aspect_16_9":
			AspectRatio = AspectRatio.Aspect_16_9;
			break;
		case "Aspect_21_9":
			AspectRatio = AspectRatio.Aspect_21_9;
			break;
		};
		MaxFps = Convert.ToInt32( config[ "Video:MaxFps" ] );
		ShadowQuality = (ShadowQuality)Convert.ToUInt32( config[ "Video:ShadowQuality" ] );
		ShadowFilterQuality = (ShadowFilterQuality)Convert.ToUInt32( config[ "Video:ShadowFilterQuality" ] );
		AntiAliasing = (AntiAliasing)Convert.ToUInt32( config[ "Video:AntiAliasing" ] );
		VSyncMode = (VSyncMode)Convert.ToUInt32( config[ "Video:VSync" ] );
		BloomEnabled = Convert.ToBoolean( config[ "Video:Bloom" ] );
		ShowFPS = Convert.ToBoolean( config[ "Video:ShowFPS" ] );
		ShowBlood = Convert.ToBoolean( config[ "Video:ShowBlood" ] );

		Instance.ApplyVideoSettings();
	}
	private static void SaveVideoSettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Video]" );
		writer.WriteLine( string.Format( "WindowMode={0}", WindowMode ) );
		writer.WriteLine( string.Format( "Resolution={0}", Resolution ) );
		writer.WriteLine( string.Format( "AspectRatio={0}", AspectRatio ) );
		writer.WriteLine( string.Format( "MaxFps={0}", MaxFps ) );
		writer.WriteLine( string.Format( "ShadowQuality={0}", (int)ShadowQuality ) );
		writer.WriteLine( string.Format( "ShadowFilterQuality={0}", (int)ShadowFilterQuality ) );
		writer.WriteLine( string.Format( "AntiAliasing={0}", (int)AntiAliasing ) );
		writer.WriteLine( string.Format( "VSync={0}", (int)VSyncMode ) );
		writer.WriteLine( string.Format( "Bloom={0}", Convert.ToUInt32( BloomEnabled ) ) );
		writer.WriteLine( string.Format( "ShowFPS={0}", Convert.ToUInt32( ShowFPS ) ) );
		writer.WriteLine( string.Format( "ShowBlood={0}", Convert.ToUInt32( ShowBlood ) ) );
		writer.WriteLine();
	}
	private static void LoadAccessibilitySettings( IDictionary<string, string> config ) {
		ColorblindMode = Convert.ToInt32( config[ "Accessibility:ColorblindMode" ] );
		HapticStrength = (float)Convert.ToDouble( config[ "Accessibility:HapticStrength" ] );
		HapticEnabled = Convert.ToBoolean( config[ "Accessibility:HapticEnabled" ] );
		AutoAimEnabled = Convert.ToBoolean( config[ "Accessibility:AutoAimEnabled" ] );
		DyslexiaMode = Convert.ToBoolean( config[ "Accessibility:DyslexiaMode" ] );
		QuicktimeAutocomplete = Convert.ToBoolean( config[ "Accessibility:QuicktimeAutocomplete" ] );
		EnableTutorials = Convert.ToBoolean( config[ "Accessibility:TutorialsEnabled" ] );
	}
	private static void SaveAccessibilitySettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Accessibility]" );
		writer.WriteLine( string.Format( "ColorblindMode={0}", ColorblindMode ) );
		writer.WriteLine( string.Format( "HapticStrength={0}", HapticStrength ) );
		writer.WriteLine( string.Format( "HapticEnabled={0}", HapticEnabled.ToString() ) );
		writer.WriteLine( string.Format( "AutoAimEnabled={0}", AutoAimEnabled.ToString() ) );
		writer.WriteLine( string.Format( "DyslexiaMode={0}", DyslexiaMode.ToString() ) );
		writer.WriteLine( string.Format( "QuicktimeAutocomplete={0}", QuicktimeAutocomplete.ToString() ) );
		writer.WriteLine( string.Format( "TutorialsEnabled={0}", EnableTutorials ) );
		writer.WriteLine();
	}
	private static void LoadGameplaySettings( IDictionary<string, string> config ) {
		EquipWeaponOnPickup = Convert.ToBoolean( config[ "Gameplay:EquipWeaponOnPickup" ] );
		HellbreakerEnabled = Convert.ToBoolean( config[ "Gameplay:HellbreakerEnabled" ] );
		HellbreakerRevanents = Convert.ToBoolean( config[ "Gameplay:HellbreakerRevanents" ] );
		CleanAudio = Convert.ToBoolean( config[ "Gameplay:CleanAudio" ] );
	}
	private static void SaveGameplaySettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Gameplay]" );
		writer.WriteLine( string.Format( "EquipWeaponOnPickup={0}", EquipWeaponOnPickup.ToString() ) );
		writer.WriteLine( string.Format( "HellbreakerEnabled={0}", HellbreakerEnabled.ToString() ) );
		writer.WriteLine( string.Format( "HellbreakerRevanents={0}", HellbreakerRevanents.ToString() ) );
		writer.WriteLine( string.Format( "CleanAudio={0}", CleanAudio.ToString() ) );
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
		Default = ResourceLoader.Load<DefaultSettings>( "res://resources/DefaultSettings.tres" );

		WindowMode = Default.WindowMode;
		AspectRatio = Default.AspectRatio;
		Resolution = Default.Resolution;
		VSyncMode = Default.Vsync;
		AntiAliasing = Default.AntiAliasing;
		MaxFps = Default.MaxFps;
		BloomEnabled = Default.BloomEnabled;
		SetShowFPS( Default.ShowFps );
		ShowBlood = Default.ShowBlood;

		HapticStrength = Default.HapticStrength;
		HapticEnabled = Default.HapticFeedback;
		QuicktimeAutocomplete = Default.QuicktimeAutocomplete;
		ColorblindMode = Default.ColorblindMode;
		AutoAimEnabled = Default.AutoAim;
		DyslexiaMode = Default.DyslexiaMode;
		EnableTutorials = Default.EnableTutorials;

		EffectsOn = Default.SoundEffectsOn;
		SetEffectsVolume( Default.SoundEffectsVolume );
		MusicOn = Default.MusicOn;
		SetMusicVolume( Default.MusicVolume );
		MuteUnfocused = Default.MuteUnfocused;

		EquipWeaponOnPickup = Default.EquipWeaponOnPickup;
		HellbreakerEnabled = Default.Hellbreaker;
		HellbreakerRevanents = Default.HellbreakerRevanents;
		CleanAudio = Default.CleanAudio;

		EnableNetworking = Default.NetworkingEnabled;
		FriendsOnlyNetworking = Default.FriendsOnly;
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
			ApplyVideoSettings();
			return;
		}

		IDictionary<string, string> iniData = IniStreamConfigurationProvider.Read( stream );
		try {
			LoadVideoSettings( iniData );
			LoadAudioSettings( iniData );
			LoadAccessibilitySettings( iniData );
			LoadGameplaySettings( iniData );
			LoadNetworkingSettings( iniData );
		} catch ( Exception e ) {
			Console.PrintWarning( string.Format( "Exception while loading settings {0}, using defaults.", e.Message ) );
			GetDefaultConfig();
		}

		if ( iniData.TryGetValue( "Internal:LastSaveSlot", out string value ) ) {
			LastSaveSlot = Convert.ToInt32( value );
		}

		ApplyVideoSettings();

		Console.PrintLine( "...Finished applying settings" );
	}

	public static void Save() {
		Console.PrintLine( "Saving configuration data..." );

		string path = ProjectSettings.GlobalizePath( "user://settings.ini" );

		DirAccess.RemoveAbsolute( path );
		using ( var stream = new System.IO.FileStream( path, System.IO.FileMode.Create ) ) {
			using ( var writer = new System.IO.StreamWriter( stream ) ) {
				SaveVideoSettings( writer );
				SaveAudioSettings( writer );
				SaveAccessibilitySettings( writer );
				SaveGameplaySettings( writer );
				SaveNetworkingSettings( writer );

				writer.WriteLine( string.Format( "[Internal]" ) );
				writer.WriteLine( string.Format( "LastSaveSlot={0}", LastSaveSlot ) );

				writer.Flush();

				ResourceSaver.Save( RemappingConfig, "user://input_context.tres" );
			}
		}

		Instance.EmitSignalSettingsChanged();
	}
};