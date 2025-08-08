using Microsoft.Extensions.Configuration.Ini;
using Godot;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

public partial class SettingsData : Node {
	public static DefaultSettings Default { get; private set; }

	//
	// video options
	//
	private static WindowMode WindowMode;
	private static DRSPreset DRSPreset;
	private static AspectRatio AspectRatio;
	private static Resolution Resolution;
	private static ShadowQuality ShadowQuality;
	private static ShadowFilterQuality ShadowFilterQuality;
	private static ParticleQuality ParticleQuality;
	private static LightingQuality LightingQuality;
	private static AnimationQuality AnimationQuality;
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
	private static ColorblindMode ColorblindMode;
	private static AutoAimMode AutoAimMode;
	private static bool DyslexiaMode;
	private static float UIScale;
	private static bool TextToSpeech;
	private static int TtsVoiceIndex;
	private static bool EnableTutorials;
	private static HUDPreset HUDPreset;

	//
	// audio options
	//
	private static bool EffectsOn;
	private static float EffectsVolume;
	private static bool MusicOn;
	private static float MusicVolume;
	private static bool MuteUnfocused;

	//
	// gameplay optionsSettingsData.GetShadowQuality()
	//
	private static bool EquipWeaponOnPickup;
	private static bool HellbreakerEnabled;
	private static bool HellbreakerRevanents;

	//
	// network options
	//
	private static bool EnableNetworking;
	private static bool FriendsOnlyNetworking;

	private static float EffectsVolumeDb_Flat = 0.0f;
	private static float MusicVolumeDb_Flat = 0.0f;
	private static float ShadowFilterSmooth = 0.0f;
	private static Light2D.ShadowFilterEnum ShadowFilterType = Light2D.ShadowFilterEnum.None;

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

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static WindowMode GetWindowMode() => WindowMode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetWindowMode( WindowMode mode ) => WindowMode = mode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static DRSPreset GetDRSPreset() => DRSPreset;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetDRSPreset( DRSPreset preset ) => DRSPreset = preset;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetDRSTargetFrames() => DRSTargetFrames;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetDRSTargetFrames( int nFrames ) => DRSTargetFrames = nFrames;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Resolution GetResolution() => Resolution;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetResolution( Resolution resolution ) => Resolution = resolution;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetMaxFps() => MaxFps;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetMaxFps( int nMaxFps ) {
		MaxFps = nMaxFps;
		Engine.MaxFps = MaxFps;
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static LightingQuality GetLightingQuality() => LightingQuality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetLightingQuality( LightingQuality quality ) => LightingQuality = quality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ShadowQuality GetShadowQuality() => ShadowQuality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetShadowQuality( ShadowQuality quality ) => ShadowQuality = quality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ShadowFilterQuality GetShadowFilterQuality() => ShadowFilterQuality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetShadowFilterQuality( ShadowFilterQuality quality ) => ShadowFilterQuality = quality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Light2D.ShadowFilterEnum GetShadowFilterEnum() => ShadowFilterType;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float GetShadowFilterSmooth() => ShadowFilterSmooth;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ParticleQuality GetParticleQuality() => ParticleQuality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetParticleQuality( ParticleQuality quality ) => ParticleQuality = quality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static AnimationQuality GetAnimationQuality() => AnimationQuality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetAnimationQuality( AnimationQuality quality ) => AnimationQuality = quality;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static VSyncMode GetVSync() => VSyncMode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetVSync( VSyncMode vsync ) => VSyncMode = vsync;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static AntiAliasing GetAntiAliasing() => AntiAliasing;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetAntiAliasing( AntiAliasing mode ) => AntiAliasing = mode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetBloomEnabled() => BloomEnabled;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetBloomEnabled( bool bBloomEnabled ) => BloomEnabled = bBloomEnabled;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetShowFPS() => ShowFPS;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetShowFPS( bool bShowFPS ) {
		ShowFPS = bShowFPS;
		Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ShowFPS ? ProcessModeEnum.Always : ProcessModeEnum.Disabled;
		Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = ShowFPS;
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetShowBlood() => ShowBlood;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetShowBlood( bool bShowBlood ) => ShowBlood = bShowBlood;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetEffectsOn() => EffectsOn;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetEffectsOn( bool bEffectsOn ) => EffectsOn = bEffectsOn;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float GetEffectsVolume() => EffectsVolume;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float GetEffectsVolumeLinear() => EffectsVolumeDb_Flat;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetEffectsVolume( float fEffectsVolume ) {
		EffectsVolume = fEffectsVolume;
		EffectsVolumeDb_Flat = Mathf.LinearToDb( EffectsVolume * 0.01f );
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetMusicOn() => MusicOn;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetMusicOn( bool bMusicOn ) => MusicOn = bMusicOn;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float GetMusicVolume() => MusicVolume;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float GetMusicVolumeLinear() => MusicVolumeDb_Flat;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetMusicVolume( float fMusicVolume ) {
		MusicVolume = fMusicVolume;
		MusicVolumeDb_Flat = Mathf.LinearToDb( MusicVolume * 0.01f );
	}
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetMuteUnfocused() => MuteUnfocused;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetMuteUnfocused( bool bMuteUnfocused ) => MuteUnfocused = bMuteUnfocused;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetHapticEnabled() => HapticEnabled;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetHapticEnabled( bool bHapticEnabled ) => HapticEnabled = bHapticEnabled;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float GetHapticStrength() => HapticStrength;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetHapticStrength( float fHapticStrength ) => HapticStrength = fHapticStrength;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static AutoAimMode GetAutoAimMode() => AutoAimMode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetAutoAimMode( AutoAimMode mode ) => AutoAimMode = mode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static ColorblindMode GetColorblindMode() => ColorblindMode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetColorblindMode( ColorblindMode mode ) => ColorblindMode = mode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetTutorialsEnabled() => EnableTutorials;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetTutorialsEnabled( bool bTutorialsEnabled ) => EnableTutorials = bTutorialsEnabled;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetDyslexiaMode() => DyslexiaMode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetDyslexiaMode( bool bDyslexiaMode ) => DyslexiaMode = bDyslexiaMode;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float GetUIScale() => UIScale;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetUIScale( float nScale ) => UIScale = nScale;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetTextToSpeech() => TextToSpeech;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetTextToSpeech( bool bTextToSpeech ) => TextToSpeech = bTextToSpeech;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetTtsVoiceIndex() => TtsVoiceIndex;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetTtsVoiceIndex( int nVoiceIndex ) => TtsVoiceIndex = nVoiceIndex;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static HUDPreset GetHUDPreset() => HUDPreset;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetHUDPreset( HUDPreset preset ) => HUDPreset = preset;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetEquipWeaponOnPickup() => EquipWeaponOnPickup;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetEquipWeaponOnPickup( bool bEquipWeapon ) => EquipWeaponOnPickup = bEquipWeapon;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool GetHellbreakerEnabled() => HellbreakerEnabled;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetHellbreakerEnabled( bool bHellbreakerEnabled ) => HellbreakerEnabled = bHellbreakerEnabled;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int GetSaveSlot() => LastSaveSlot;
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static void SetSaveSlot( int nSlot ) => LastSaveSlot = nSlot;

	private void UpdateWindowScale() {
		Godot.Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
		Godot.Vector2I windowSize = GetWindow().GetSizeWithDecorations();
		GetWindow().SetImePosition( centerScreen - windowSize / 2 );
	}
	public static void ApplyVideoSettings() {
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
			//			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Mailbox );
			DisplayServer.WindowSetVsyncMode( DisplayServer.VSyncMode.Enabled );
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
		Instance.GetWindow().Size = windowSize;

		Engine.MaxFps = MaxFps;

		Rid viewport = Instance.GetTree().Root.GetViewportRid();
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

		switch ( LightingQuality ) {
		case LightingQuality.VeryLow:
			ProjectSettings.SetSetting( "rendering/shading/force_lambert_over_burley", true );
			break;
		case LightingQuality.Low:
			ProjectSettings.SetSetting( "rendering/shading/force_lambert_over_burley", true );
			break;
		case LightingQuality.High:
			ProjectSettings.SetSetting( "rendering/shading/force_lambert_over_burley", false );
			break;
		default:
			Console.PrintError( string.Format( "SettingsData.ApplyVideoSettings: invalid LightingQuality ({0}) - setting to default", LightingQuality ) );

			ProjectSettings.SetSetting( "rendering/shading/force_lambert_over_burley", true );
			LightingQuality = LightingQuality.Low;
			break;
		};

		switch ( ShadowQuality ) {
		case ShadowQuality.Off:
			RenderingServer.CanvasSetShadowTextureSize( 0 );
			break;
		case ShadowQuality.Low:
			ProjectSettings.SetSetting( "rendering/lights_and_shadows/tighter_shadow_caster_culling", true );
			RenderingServer.CanvasSetShadowTextureSize( 2048 );
			break;
		case ShadowQuality.Medium:
			ProjectSettings.SetSetting( "rendering/lights_and_shadows/tighter_shadow_caster_culling", false );
			RenderingServer.CanvasSetShadowTextureSize( 4096 );
			break;
		case ShadowQuality.High:
			ProjectSettings.SetSetting( "rendering/lights_and_shadows/tighter_shadow_caster_culling", false );
			RenderingServer.CanvasSetShadowTextureSize( 8192 );
			break;
		case ShadowQuality.Ultra:
			ProjectSettings.SetSetting( "rendering/lights_and_shadows/tighter_shadow_caster_culling", false );
			RenderingServer.CanvasSetShadowTextureSize( 12288 );
			break;
		default:
			Console.PrintError( string.Format( "SettingsData.ApplyVideoSettings: invalid ShadowQuality ({0}) - setting to default", ShadowQuality ) );
			ProjectSettings.SetSetting( "rendering/lights_and_shadows/tighter_shadow_caster_culling", false );
			RenderingServer.CanvasSetShadowTextureSize( 4096 );

			ShadowQuality = ShadowQuality.Medium;
			break;
		};

		switch ( ShadowFilterQuality ) {
		case ShadowFilterQuality.Off:
			ShadowFilterType = Light2D.ShadowFilterEnum.None;
			ShadowFilterSmooth = 0.0f;
			break;
		case ShadowFilterQuality.Low:
			ShadowFilterType = Light2D.ShadowFilterEnum.Pcf5;
			ShadowFilterSmooth = 0.10f;
			break;
		case ShadowFilterQuality.High:
			ShadowFilterType = Light2D.ShadowFilterEnum.Pcf13;
			ShadowFilterSmooth = 0.20f;
			break;
		default:
			Console.PrintError( string.Format( "SettingsData.ApplyVideoSettings: invalid ShadowFilterQuality ({0}) - setting to default", ShadowQuality ) );
			ShadowFilterType = Light2D.ShadowFilterEnum.Pcf5;
			ShadowFilterSmooth = 0.10f;

			ShadowFilterQuality = ShadowFilterQuality.Low;
			break;
		};

		Instance.UpdateWindowScale();
	}

	private static void LoadAudioSettings( IDictionary<string, string> config ) {
		EffectsOn = Convert.ToBoolean( config[ "Audio:SFXEnabled" ].ToInt() );
		SetEffectsVolume( (float)Convert.ToDouble( config[ "Audio:SFXVolume" ] ) );
		MusicOn = Convert.ToBoolean( config[ "Audio:MusicEnabled" ].ToInt() );
		SetMusicVolume( (float)Convert.ToDouble( config[ "Audio:MusicVolume" ] ) );
	}
	private static void SaveAudioSettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Audio]" );
		writer.WriteLine( string.Format( "SFXEnabled={0}", Convert.ToInt32( EffectsOn ) ) );
		writer.WriteLine( string.Format( "SFXVolume={0}", EffectsVolume ) );
		writer.WriteLine( string.Format( "MusicEnabled={0}", Convert.ToInt32( MusicOn ) ) );
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
		ParticleQuality = (ParticleQuality)Convert.ToInt32( config[ "Video:ParticleQuality" ] );
		AnimationQuality = (AnimationQuality)Convert.ToInt32( config[ "Video:AnimationQuality" ] );
		ShadowQuality = (ShadowQuality)Convert.ToUInt32( config[ "Video:ShadowQuality" ] );
		ShadowFilterQuality = (ShadowFilterQuality)Convert.ToUInt32( config[ "Video:ShadowFilterQuality" ] );
		LightingQuality = (LightingQuality)Convert.ToUInt32( config[ "Video:LightingQuality" ] );
		AntiAliasing = (AntiAliasing)Convert.ToUInt32( config[ "Video:AntiAliasing" ] );
		VSyncMode = (VSyncMode)Convert.ToInt32( config[ "Video:VSync" ] );
		BloomEnabled = Convert.ToBoolean( config[ "Video:Bloom" ].ToInt() );
		ShowFPS = Convert.ToBoolean( config[ "Video:ShowFPS" ].ToInt() );
		ShowBlood = Convert.ToBoolean( config[ "Video:ShowBlood" ].ToInt() );

		SettingsData.ApplyVideoSettings();
	}
	private static void SaveVideoSettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Video]" );
		writer.WriteLine( string.Format( "WindowMode={0}", WindowMode ) );
		writer.WriteLine( string.Format( "Resolution={0}", Resolution ) );
		writer.WriteLine( string.Format( "AspectRatio={0}", AspectRatio ) );
		writer.WriteLine( string.Format( "MaxFps={0}", MaxFps ) );
		writer.WriteLine( string.Format( "ParticleQuality={0}", (int)ParticleQuality ) );
		writer.WriteLine( string.Format( "ShadowQuality={0}", (int)ShadowQuality ) );
		writer.WriteLine( string.Format( "ShadowFilterQuality={0}", (int)ShadowFilterQuality ) );
		writer.WriteLine( string.Format( "LightingQuality={0}", (int)LightingQuality ) );
		writer.WriteLine( string.Format( "AnimationQuality={0}", (int)AnimationQuality ) );
		writer.WriteLine( string.Format( "AntiAliasing={0}", (int)AntiAliasing ) );
		writer.WriteLine( string.Format( "VSync={0}", (int)VSyncMode ) );
		writer.WriteLine( string.Format( "Bloom={0}", Convert.ToInt32( BloomEnabled ) ) );
		writer.WriteLine( string.Format( "ShowFPS={0}", Convert.ToInt32( ShowFPS ) ) );
		writer.WriteLine( string.Format( "ShowBlood={0}", Convert.ToInt32( ShowBlood ) ) );
		writer.WriteLine();
	}
	private static void LoadAccessibilitySettings( IDictionary<string, string> config ) {
		ColorblindMode = (ColorblindMode)Convert.ToInt32( config[ "Accessibility:ColorblindMode" ] );
		HapticStrength = Convert.ToSingle( config[ "Accessibility:HapticStrength" ] );
		HapticEnabled = Convert.ToBoolean( config[ "Accessibility:HapticEnabled" ].ToInt() );
		AutoAimMode = (AutoAimMode)Convert.ToUInt32( config[ "Accessibility:AutoAimMode" ] );
		DyslexiaMode = Convert.ToBoolean( config[ "Accessibility:DyslexiaMode" ].ToInt() );
		QuicktimeAutocomplete = Convert.ToBoolean( config[ "Accessibility:QuicktimeAutocomplete" ].ToInt() );
		EnableTutorials = Convert.ToBoolean( config[ "Accessibility:TutorialsEnabled" ].ToInt() );
		TextToSpeech = Convert.ToBoolean( config[ "Accessibility:TextToSpeech" ].ToInt() );
		TtsVoiceIndex = Convert.ToInt32( config[ "Accessibility:TtsVoiceIndex" ] );
		HUDPreset = (HUDPreset)Convert.ToInt32( config[ "Accessibility:HUDPreset" ] );
	}
	private static void SaveAccessibilitySettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Accessibility]" );
		writer.WriteLine( string.Format( "ColorblindMode={0}", (int)ColorblindMode ) );
		writer.WriteLine( string.Format( "HapticStrength={0}", HapticStrength ) );
		writer.WriteLine( string.Format( "HapticEnabled={0}", Convert.ToInt32( HapticEnabled ) ) );
		writer.WriteLine( string.Format( "AutoAimMode={0}", (uint)AutoAimMode ) );
		writer.WriteLine( string.Format( "DyslexiaMode={0}", Convert.ToInt32( DyslexiaMode ) ) );
		writer.WriteLine( string.Format( "QuicktimeAutocomplete={0}", Convert.ToInt32( QuicktimeAutocomplete ) ) );
		writer.WriteLine( string.Format( "TutorialsEnabled={0}", Convert.ToInt32( EnableTutorials ) ) );
		writer.WriteLine( string.Format( "TextToSpeech={0}", Convert.ToInt32( TextToSpeech ) ) );
		writer.WriteLine( string.Format( "TtsVoiceIndex={0}", TtsVoiceIndex ) );
		writer.WriteLine( string.Format( "EnableTutorials={0}", Convert.ToInt32( EnableTutorials ) ) );
		writer.WriteLine( string.Format( "HUDPreset={0}", (uint)HUDPreset ) );
		writer.WriteLine();
	}
	private static void LoadGameplaySettings( IDictionary<string, string> config ) {
		EquipWeaponOnPickup = Convert.ToBoolean( config[ "Gameplay:EquipWeaponOnPickup" ].ToInt() );
		HellbreakerEnabled = Convert.ToBoolean( config[ "Gameplay:HellbreakerEnabled" ].ToInt() );
		HellbreakerRevanents = Convert.ToBoolean( config[ "Gameplay:HellbreakerRevanents" ].ToInt() );
	}
	private static void SaveGameplaySettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Gameplay]" );
		writer.WriteLine( string.Format( "EquipWeaponOnPickup={0}", Convert.ToInt32( EquipWeaponOnPickup ) ) );
		writer.WriteLine( string.Format( "HellbreakerEnabled={0}", Convert.ToInt32( HellbreakerEnabled ) ) );
		writer.WriteLine( string.Format( "HellbreakerRevanents={0}", Convert.ToInt32( HellbreakerRevanents ) ) );
		writer.WriteLine();
	}
	private static void LoadNetworkingSettings( IDictionary<string, string> config ) {
		EnableNetworking = Convert.ToBoolean( config[ "Networking:EnableNetworking" ].ToInt() );
		FriendsOnlyNetworking = Convert.ToBoolean( config[ "Networking:FriendsOnlyNetworking" ].ToInt() );
	}
	private static void SaveNetworkingSettings( System.IO.StreamWriter writer ) {
		writer.WriteLine( "[Networking]" );
		writer.WriteLine( string.Format( "EnableNetworking={0}", Convert.ToInt32( EnableNetworking ) ) );
		writer.WriteLine( string.Format( "FriendsOnlyNetworking={0}", Convert.ToInt32( FriendsOnlyNetworking ) ) );
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
		ShadowQuality = Default.ShadowQuality;
		ShadowFilterQuality = Default.ShadowFilterQuality;
		ParticleQuality = Default.ParticleQuality;
		LightingQuality = Default.LightingQuality;
		BloomEnabled = Default.BloomEnabled;
		SetShowFPS( Default.ShowFps );
		ShowBlood = Default.ShowBlood;

		HapticStrength = Default.HapticStrength;
		HapticEnabled = Default.HapticFeedback;
		QuicktimeAutocomplete = Default.QuicktimeAutocomplete;
		ColorblindMode = Default.ColorblindMode;
		AutoAimMode = Default.AutoAim;
		DyslexiaMode = Default.DyslexiaMode;
		EnableTutorials = Default.EnableTutorials;
		HUDPreset = Default.HUDPreset;
		TextToSpeech = Default.TextToSpeech;
		TtsVoiceIndex = Default.TtsVoiceIndex;

		EffectsOn = Default.SoundEffectsOn;
		SetEffectsVolume( Default.SoundEffectsVolume );
		MusicOn = Default.MusicOn;
		SetMusicVolume( Default.MusicVolume );

		EquipWeaponOnPickup = Default.EquipWeaponOnPickup;
		HellbreakerEnabled = Default.Hellbreaker;
		HellbreakerRevanents = Default.HellbreakerRevanents;

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
			Console.PrintError( string.Format( "Exception while loading settings {0} - using defaults.", e.Message ) );
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