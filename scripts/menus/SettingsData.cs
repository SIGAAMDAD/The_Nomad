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
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

namespace Menus {
	/*
	===================================================================================

	SettingsData

	===================================================================================
	*/

	public partial class SettingsData : Node {
		private static readonly string ConfigSaveFile = "user://settings.ini";

		//
		// video options
		//
		public static WindowMode WindowMode { get; private set; }
		public static DRSPreset DRSPreset { get; private set; }
		public static AspectRatio AspectRatio { get; private set; }
		public static WindowResolution Resolution { get; private set; }
		public static ShadowQuality ShadowQuality { get; private set; }
		public static ShadowFilterQuality ShadowFilterQuality { get; private set; }
		public static ParticleQuality ParticleQuality { get; private set; }
		public static LightingQuality LightingQuality { get; private set; }
		public static AnimationQuality AnimationQuality { get; private set; }
		public static VSyncMode VSyncMode { get; private set; }
		public static AntiAliasing AntiAliasing { get; private set; }
		public static int MaxFps { get; private set; }
		public static int DRSTargetFrames { get; private set; }
		public static bool BloomEnabled { get; private set; }
		public static PerformanceOverlayPreset PerformanceOverlay { get; private set; }
		public static bool ShowBlood { get; private set; }

		//
		// accessibility options
		//
		public static float HapticStrength { get; private set; }
		public static bool HapticEnabled { get; private set; }
		public static bool QuicktimeAutocomplete { get; private set; }
		public static ColorblindMode ColorblindMode { get; private set; }
		public static AutoAimMode AutoAimMode { get; private set; }
		public static bool DyslexiaMode { get; private set; }
		public static float UIScale { get; private set; }
		public static bool TextToSpeech { get; private set; }
		public static int TtsVoiceIndex { get; private set; }
		public static bool EnableTutorials { get; private set; }
		public static HUDPreset HUDPreset { get; private set; }

		//
		// audio options
		//
		public static bool EffectsOn { get; private set; }
		public static float EffectsVolume { get; private set; }
		public static bool MusicOn { get; private set; }
		public static float MusicVolume { get; private set; }

		//
		// gameplay options
		//
		public static bool EquipWeaponOnPickup { get; private set; }
		private static bool HellbreakerEnabled;
		private static bool HellbreakerRevanents;

		//
		// network options
		//
		public static bool EnableNetworking { get; private set; }
		public static bool CODLobbies { get; private set; }
		public static bool BountyHuntEnabled { get; private set; }

		private static float EffectsVolumeDb_Flat = 0.0f;
		private static float MusicVolumeDb_Flat = 0.0f;
		public static float ShadowFilterSmooth { get; private set; } = 0.0f;
		public static Light2D.ShadowFilterEnum ShadowFilterType { get; private set; } = Light2D.ShadowFilterEnum.None;

		private static Godot.Collections.Array<Resource> MappingContexts;
		public static RefCounted Remapper { get; private set; }
		public static Resource RemappingConfig { get; private set; }
		public static Godot.Collections.Array<RefCounted> RemappableItems { get; private set; }
		public static RefCounted MappingFormatter { get; private set; }

		public static int LastSaveSlot { get; private set; } = 0;

		private static readonly IReadOnlyDictionary<WindowResolution, Vector2I> WindowResolutions = new Dictionary<WindowResolution, Vector2I>{
			{ WindowResolution.Res_640x480, new Vector2I( 640, 480 ) },
			{ WindowResolution.Res_800x600, new Vector2I( 800, 600 ) },
			{ WindowResolution.Res_1024x768, new Vector2I( 1024, 768 ) },
			{ WindowResolution.Res_1280x720, new Vector2I( 1280, 720 ) },
			{ WindowResolution.Res_1280x768, new Vector2I( 1280, 768 ) },
			{ WindowResolution.Res_1280x800, new Vector2I( 1280, 800 ) },
			{ WindowResolution.Res_1280x1024, new Vector2I( 1280, 1024 ) },
			{ WindowResolution.Res_1360x768, new Vector2I( 1360, 768 ) },
			{ WindowResolution.Res_1366x768, new Vector2I( 1366, 768 ) },
			{ WindowResolution.Res_1440x900, new Vector2I( 1440, 900 ) },
			{ WindowResolution.Res_1536x864, new Vector2I( 1536, 864 ) },
			{ WindowResolution.Res_1600x900, new Vector2I( 1600, 900 ) },
			{ WindowResolution.Res_1600x1200, new Vector2I( 1600, 1200 ) },
			{ WindowResolution.Res_1680x1050, new Vector2I( 1680, 1050 ) },
			{ WindowResolution.Res_1920x1080, new Vector2I( 1920, 1080 ) },
			{ WindowResolution.Res_1920x1200, new Vector2I( 1920, 1200 ) },
			{ WindowResolution.Res_2048x1152, new Vector2I( 2048, 1152 ) },
			{ WindowResolution.Res_2048x1536, new Vector2I( 2048, 1536 ) },
			{ WindowResolution.Res_2560x1080, new Vector2I( 2560, 1080 ) },
			{ WindowResolution.Res_2560x1440, new Vector2I( 2560, 1440 ) },
			{ WindowResolution.Res_2560x1600, new Vector2I( 2560, 1600 ) },
			{ WindowResolution.Res_3440x1440, new Vector2I( 3440, 1440 ) },
			{ WindowResolution.Res_3840x2160, new Vector2I( 3840, 2160 ) }
		};

		public static SettingsData Instance = null;

		[Signal]
		public delegate void SettingsChangedEventHandler();

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetNetworkingEnabled( bool networking ) => EnableNetworking = networking;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetBountyHuntEnabled( bool huntEnabled ) => BountyHuntEnabled = huntEnabled;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetCODLobbyEnabled( bool lobbyEnabled ) => CODLobbies = lobbyEnabled;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetWindowMode( WindowMode mode ) => WindowMode = mode;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetDRSPreset( DRSPreset preset ) => DRSPreset = preset;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetDRSTargetFrames( int frames ) => DRSTargetFrames = frames;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetResolution( WindowResolution resolution ) => Resolution = resolution;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetMaxFps( int maxFps ) {
			MaxFps = maxFps;
			Engine.MaxFps = MaxFps;
	}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetLightingQuality( LightingQuality quality ) => LightingQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetShadowQuality( ShadowQuality quality ) => ShadowQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetShadowFilterQuality( ShadowFilterQuality quality ) => ShadowFilterQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetParticleQuality( ParticleQuality quality ) => ParticleQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetAnimationQuality( AnimationQuality quality ) => AnimationQuality = quality;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetVSync( VSyncMode vsync ) => VSyncMode = vsync;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetAntiAliasing( AntiAliasing mode ) => AntiAliasing = mode;

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetBloomEnabled( bool bloomEnabled ) => BloomEnabled = bloomEnabled;

		/*
		===============
		SetPerformanceOverlay
		===============
		*/
		public static void SetPerformanceOverlay( PerformanceOverlayPreset overlayPreset ) {
			PerformanceOverlay = overlayPreset;

			if ( overlayPreset == PerformanceOverlayPreset.Hidden ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Disabled;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = false;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 0 );
			} else if ( overlayPreset == PerformanceOverlayPreset.FpsOnly ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Always;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = true;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 0 );
			} else if ( overlayPreset == PerformanceOverlayPreset.Partial ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Disabled;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = false;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 1 );
			} else if ( overlayPreset == PerformanceOverlayPreset.Full ) {
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).ProcessMode = ProcessModeEnum.Disabled;
				Instance.GetNode<CanvasLayer>( "/root/FpsCounter" ).Visible = false;

				Instance.GetNode<CanvasLayer>( "/root/DebugMenu" ).Set( "style", 2 );
			}
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetShowBlood( bool showBlood ) => ShowBlood = showBlood;

		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetEffectsOn( bool effectsOn ) => EffectsOn = effectsOn;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static float GetEffectsVolumeLinear() => EffectsVolumeDb_Flat;
		public static void SetEffectsVolume( float effectsVolume ) {
			EffectsVolume = effectsVolume;
			EffectsVolumeDb_Flat = Mathf.LinearToDb( EffectsVolume * 0.01f );
		}
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetMusicOn( bool musicOn ) => MusicOn = musicOn;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static float GetMusicVolumeLinear() => MusicVolumeDb_Flat;
		public static void SetMusicVolume( float musicVolume ) {
			MusicVolume = musicVolume;
			MusicVolumeDb_Flat = Mathf.LinearToDb( MusicVolume * 0.01f );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetHapticEnabled( bool hapticEnabled ) => HapticEnabled = hapticEnabled;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetHapticStrength( float hapticStrength ) => HapticStrength = hapticStrength;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetAutoAimMode( AutoAimMode mode ) => AutoAimMode = mode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetColorblindMode( ColorblindMode mode ) => ColorblindMode = mode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetTutorialsEnabled( bool tutorialsEnabled ) => EnableTutorials = tutorialsEnabled;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetDyslexiaMode( bool dyslexiaMode ) => DyslexiaMode = dyslexiaMode;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetUIScale( float scale ) => UIScale = scale;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetTextToSpeech( bool textToSpeech ) => TextToSpeech = textToSpeech;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetTtsVoiceIndex( int voiceIndex ) => TtsVoiceIndex = voiceIndex;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetHUDPreset( HUDPreset preset ) => HUDPreset = preset;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetEquipWeaponOnPickup( bool equipWeapon ) => EquipWeaponOnPickup = equipWeapon;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetHellbreakerEnabled( bool hellbreakerEnabled ) => HellbreakerEnabled = hellbreakerEnabled;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static void SetSaveSlot( int slot ) => LastSaveSlot = slot;

		/*
		===============
		UpdateWindowScale
		===============
		*/
		/// <summary>
		/// Updates window position based on the size
		/// </summary>
		private void UpdateWindowScale() {
			Godot.Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
			Godot.Vector2I windowSize = GetWindow().GetSizeWithDecorations();
			GetWindow().SetImePosition( centerScreen - windowSize / 2 );
		}

		/*
		===============
		UpdateWindowResolution
		===============
		*/
		/// <summary>
		/// Updates window resolution based on <see cref="Resolution"/>
		/// </summary>
		private static void UpdateWindowResolution() {
			if ( !WindowResolutions.TryGetValue( Resolution, out Vector2I windowSize ) ) {
				Console.PrintError( string.Format( "SettingsData.ApplyVideoSettings: invalid Resolution ({0}) - setting to default", (int)Resolution ) );

				Resolution = WindowResolution.Res_640x480;
				windowSize = WindowResolutions[ WindowResolution.Res_640x480 ];
			}
			Instance.GetWindow().Size = windowSize;
		}

		/*
		===============
		UpdateVSyncSettings
		===============
		*/
		private static void UpdateVSyncMode() {
			ArgumentNullException.ThrowIfNull( Settings.VSyncMode.DataTable );

			if ( !Settings.VSyncMode.DataTable.TryGetValue( VSyncMode, out Settings.VSyncMode mode ) ) {
				Console.PrintError( string.Format( "SettingsData.ApplyVideoSettings: invalid VSyncMode ({0}) - setting to default", (int)VSyncMode ) );
				VSyncMode = VSyncMode.Off;
				mode = Settings.VSyncMode.DataTable[ VSyncMode ];
			}

			DisplayServer.WindowSetVsyncMode( mode.Mode );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				mode.SwapChainImageCount
			);
		}

		/*
		===============
		UpdateWindowMode
		===============
		*/
		private static void UpdateWindowMode() {
			DisplayServer.WindowMode mode;
			bool borderless;

			switch ( WindowMode ) {
				case WindowMode.Windowed:
					borderless = false;
					mode = DisplayServer.WindowMode.Windowed;
					break;
				case WindowMode.BorderlessWindowed:
					borderless = true;
					mode = DisplayServer.WindowMode.Windowed;
					break;
				case WindowMode.Fullscreen:
					borderless = false;
					mode = DisplayServer.WindowMode.Fullscreen;
					break;
				case WindowMode.BorderlessFullscreen:
					borderless = true;
					mode = DisplayServer.WindowMode.Fullscreen;
					break;
				case WindowMode.ExclusiveFullscreen:
					borderless = true;
					mode = DisplayServer.WindowMode.ExclusiveFullscreen;
					break;
				default:
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid WindowMode ({WindowMode}) - setting to default" );
					WindowMode = WindowMode.Fullscreen;

					mode = DisplayServer.WindowMode.Fullscreen;
					borderless = false;
					break;
			}

			DisplayServer.WindowSetMode( mode );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, borderless );
		}

		/*
		===============
		UpdateAntiAliasing
		===============
		*/
		private static void UpdateAntiAliasing() {
			RenderingServer.ViewportMsaa viewportMsaa;
			RenderingServer.ViewportScreenSpaceAA screenSpaceAA;
			bool useTaa;

			switch ( AntiAliasing ) {
				case AntiAliasing.None:
					useTaa = false;
					viewportMsaa = RenderingServer.ViewportMsaa.Disabled;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled;
					break;
				case AntiAliasing.FXAA:
					useTaa = false;
					viewportMsaa = RenderingServer.ViewportMsaa.Disabled;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Fxaa;
					break;
				case AntiAliasing.MSAA_2x:
					useTaa = false;
					viewportMsaa = RenderingServer.ViewportMsaa.Msaa2X;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled;
					break;
				case AntiAliasing.MSAA_4x:
					useTaa = false;
					viewportMsaa = RenderingServer.ViewportMsaa.Msaa4X;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled;
					break;
				case AntiAliasing.MSAA_8x:
					useTaa = false;
					viewportMsaa = RenderingServer.ViewportMsaa.Msaa8X;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled;
					break;
				case AntiAliasing.FXAA_and_TAA:
					useTaa = true;
					viewportMsaa = RenderingServer.ViewportMsaa.Disabled;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Fxaa;
					break;
				case AntiAliasing.TAA:
					useTaa = true;
					viewportMsaa = RenderingServer.ViewportMsaa.Disabled;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled;
					break;
				default:
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid AntiAliasing ({AntiAliasing}) - setting to default" );
					AntiAliasing = AntiAliasing.None;

					useTaa = false;
					viewportMsaa = RenderingServer.ViewportMsaa.Disabled;
					screenSpaceAA = RenderingServer.ViewportScreenSpaceAA.Disabled;
					break;
			}

			Rid viewport = Instance.GetViewport().GetViewportRid();
			if ( RenderingServer.GetCurrentRenderingDriverName() == "vulkan" ) {
				RenderingServer.ViewportSetUseTaa( viewport, useTaa );
			}
			RenderingServer.ViewportSetScreenSpaceAA( viewport, screenSpaceAA );
			RenderingServer.ViewportSetMsaa2D( viewport, viewportMsaa );
		}

		/*
		===============
		UpdateLightingQuality
		===============
		*/
		private static void UpdateLightingQuality() {
			bool forceLambertOverBurley = false;

			switch ( LightingQuality ) {
				case LightingQuality.VeryLow:
					forceLambertOverBurley = true;
					break;
				case LightingQuality.Low:
					forceLambertOverBurley = false;
					break;
				case LightingQuality.High:
					forceLambertOverBurley = false;
					break;
				default:
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid LightingQuality ({LightingQuality}) - setting to default" );
					LightingQuality = LightingQuality.Low;
					break;
			}

			ProjectSettings.SetSetting( "rendering/shading/force_lambert_over_burley", forceLambertOverBurley );

			// if we're in a level, apply the changes
			if ( Instance.GetTree().CurrentScene == LevelData.Instance ) {
			}
		}

		/*
		===============
		UpdateShadowQuality
		===============
		*/
		private static void UpdateShadowQuality() {
			int shadowTextureSize;
			bool tighterShadowCasterCulling;

			switch ( ShadowQuality ) {
				case ShadowQuality.Off:
					shadowTextureSize = 0;
					tighterShadowCasterCulling = true;
					break;
				case ShadowQuality.Low:
					shadowTextureSize = 2048;
					tighterShadowCasterCulling = true;
					break;
				case ShadowQuality.Medium:
					shadowTextureSize = 4096;
					tighterShadowCasterCulling = false;
					break;
				case ShadowQuality.High:
					shadowTextureSize = 8192;
					tighterShadowCasterCulling = false;
					break;
				case ShadowQuality.Ultra:
					shadowTextureSize = 12288;
					tighterShadowCasterCulling = false;
					break;
				default:
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid ShadowQuality ({ShadowQuality}) - setting to default" );

					ShadowQuality = ShadowQuality.Medium;

					shadowTextureSize = 4096;
					tighterShadowCasterCulling = false;
					break;
			}

			RenderingServer.CanvasSetShadowTextureSize( shadowTextureSize );
			ProjectSettings.SetSetting( "rendering/lights_and_shadows/tighter_shadow_caster_culling", tighterShadowCasterCulling );
		}

		/*
		===============
		UpdateShadowFilterQuality
		===============
		*/
		private static void UpdateShadowFilterQuality() {
			// adjust filtering quality accordingly
			switch ( ShadowFilterQuality ) {
				case ShadowFilterQuality.Off: // hard shadows
					ShadowFilterType = Light2D.ShadowFilterEnum.None;
					ShadowFilterSmooth = 0.0f;
					break;
				case ShadowFilterQuality.Low: // small amounts of filtering
					ShadowFilterType = Light2D.ShadowFilterEnum.Pcf5;
					ShadowFilterSmooth = 0.10f;
					break;
				case ShadowFilterQuality.High: // full soft shadows, heaviest performance cost
					ShadowFilterType = Light2D.ShadowFilterEnum.Pcf13;
					ShadowFilterSmooth = 0.20f;
					break;
				default:
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid ShadowFilterQuality ({ShadowFilterQuality}) - setting to default" );
					ShadowFilterType = Light2D.ShadowFilterEnum.Pcf5;
					ShadowFilterSmooth = 0.10f;

					ShadowFilterQuality = ShadowFilterQuality.Low;
					break;
			}
		}

		/*
		===============
		ApplyVideoSettings
		===============
		*/
		public static void ApplyVideoSettings() {
			UpdateVSyncMode();
			UpdateWindowResolution();
			UpdateWindowMode();
			UpdateAntiAliasing();
			UpdateLightingQuality();
			UpdateShadowQuality();
			UpdateShadowFilterQuality();

			Engine.MaxFps = MaxFps;

			Instance.UpdateWindowScale();
		}

		/*
		===============
		Save
		===============
		*/
		/// <summary>
		/// Writes all configuration values to disk
		/// </summary>
		public static void Save() {
			using var configWriter = new Settings.ConfigFileWriter( ConfigSaveFile );
		}

		/*
		================
		_Ready
		===============
		*/
		/// <summary>
		/// godot initialization override
		/// </summary>
		public override void _Ready() {
			base._Ready();

			Instance = this;

			Console.PrintLine( "Loading game configuration..." );

			RefCounted bindingSetup = (RefCounted)ResourceLoader.Load<GDScript>( "res://scripts/menus/settings_bindings.gd" ).New();

			Remapper = (RefCounted)bindingSetup.Get( "_remapper" );
			RemappingConfig = (Resource)bindingSetup.Get( "_remapping_config" );
			RemappableItems = (Godot.Collections.Array<RefCounted>)bindingSetup.Get( "_remappable_items" );
			MappingFormatter = (RefCounted)bindingSetup.Get( "_mapping_formatter" );

			// load configuration file
			using var configReader = new Settings.ConfigFileReader( ConfigSaveFile );

			// apply the settings
			ApplyVideoSettings();

			Console.PrintLine( "...Finished applying settings" );
		}
	};
};