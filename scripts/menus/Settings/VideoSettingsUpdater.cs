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
using System;
using System.Collections.Generic;

namespace Menus.Settings {
	public readonly struct VideoSettingsUpdater : IDisposable {
		/// <summary>
		/// Various premade window resolutions, sourced from wikipedia
		/// </summary>
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
		
		private readonly SettingsData? Instance;

		/*
		===============
		VideoSettingsHandler
		===============
		*/
		public VideoSettingsUpdater( SettingsData? instance ) {
			ArgumentNullException.ThrowIfNull( instance );

			Instance = instance;

			UpdateVSyncMode();
			UpdateWindowResolution();
			UpdateWindowMode();
			UpdateAntiAliasing();
			UpdateLightingQuality();
			UpdateShadowQuality();
			UpdateShadowFilterQuality();

			Engine.MaxFps = SettingsData.MaxFps;

			UpdateWindowScale();
		}

		/*
		===============
		Dispose
		===============
		*/
		public void Dispose() {
		}
		
		/*
		===============
		UpdateWindowScale
		===============
		*/
		/// <summary>
		/// Updates window position based on the size
		/// </summary>
		private void UpdateWindowScale() {
			/*
			Godot.Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
			Godot.Vector2I windowSize = GetWindow().GetSizeWithDecorations();
			GetWindow().SetImePosition( centerScreen - windowSize / 2 );
			*/
			Console.PrintLine( "Updating window position..." );
			Instance.GetWindow().SetImePosition( Vector2I.Zero );
		}

		/*
		===============
		UpdateWindowResolution
		===============
		*/
		/// <summary>
		/// Updates window resolution based on <see cref="Resolution"/>
		/// </summary>
		private void UpdateWindowResolution() {
			if ( !WindowResolutions.TryGetValue( SettingsData.Resolution, out Vector2I windowSize ) ) {
				Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid Resolution ({SettingsData.Resolution}) - setting to default" );

				SettingsData.SetResolution( WindowResolution.Res_640x480 );
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
			int swapChainImageCount;
			DisplayServer.VSyncMode mode;

			switch ( SettingsData.VSyncMode ) {
				case VSyncMode.On:
					mode = DisplayServer.VSyncMode.Enabled;
					swapChainImageCount = 2;
					break;
				case VSyncMode.Off:
					mode = DisplayServer.VSyncMode.Disabled;
					swapChainImageCount = 2;
					break;
				case VSyncMode.Adaptive:
					mode = DisplayServer.VSyncMode.Adaptive;
					swapChainImageCount = 2;
					break;
				case VSyncMode.TripleBuffered:
					mode = DisplayServer.VSyncMode.Enabled;
					swapChainImageCount = 3;
					break;
				default:
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid VSyncMode ({SettingsData.VSyncMode}) - setting to default" );
					SettingsData.SetVSync( VSyncMode.Off );

					mode = DisplayServer.VSyncMode.Disabled;
					swapChainImageCount = 2;
					break;
			}

			DisplayServer.WindowSetVsyncMode( mode );
			ProjectSettings.SetSetting(
				"rendering/rendering_device/vsync/swapchain_image_count",
				swapChainImageCount
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

			switch ( SettingsData.WindowMode ) {
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
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid WindowMode ({SettingsData.WindowMode}) - setting to default" );
					SettingsData.SetWindowMode( WindowMode.Fullscreen );

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
		private void UpdateAntiAliasing() {
			RenderingServer.ViewportMsaa viewportMsaa;
			RenderingServer.ViewportScreenSpaceAA screenSpaceAA;
			bool useTaa;

			switch ( SettingsData.AntiAliasing ) {
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
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid AntiAliasing ({SettingsData.AntiAliasing}) - setting to default" );
					SettingsData.SetAntiAliasing( AntiAliasing.None );

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
		private void UpdateLightingQuality() {
			bool forceLambertOverBurley = false;

			switch ( SettingsData.LightingQuality ) {
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
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid LightingQuality ({SettingsData.LightingQuality}) - setting to default" );
					SettingsData.SetLightingQuality( LightingQuality.Low );
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

			switch ( SettingsData.ShadowQuality ) {
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
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid ShadowQuality ({SettingsData.ShadowQuality}) - setting to default" );

					SettingsData.SetShadowQuality( ShadowQuality.Medium );

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
			switch ( SettingsData.ShadowFilterQuality ) {
				case ShadowFilterQuality.Off: // hard shadows
					SettingsData.SetShadowFilterType( Light2D.ShadowFilterEnum.None );
					SettingsData.SetShadowFilterSmooth( 0.0f );
					break;
				case ShadowFilterQuality.Low: // small amounts of filtering
					SettingsData.SetShadowFilterType( Light2D.ShadowFilterEnum.Pcf5 );
					SettingsData.SetShadowFilterSmooth( 0.10f );
					break;
				case ShadowFilterQuality.High: // full soft shadows, heaviest performance cost
					SettingsData.SetShadowFilterType( Light2D.ShadowFilterEnum.Pcf13 );
					SettingsData.SetShadowFilterSmooth( 0.20f );
					break;
				default:
					Console.PrintError( $"SettingsData.ApplyVideoSettings: invalid ShadowFilterQuality ({SettingsData.ShadowFilterQuality}) - setting to default" );
					SettingsData.SetShadowFilterType( Light2D.ShadowFilterEnum.Pcf5 );
					SettingsData.SetShadowFilterSmooth( 0.10f );

					SettingsData.SetShadowFilterQuality( ShadowFilterQuality.Low );
					break;
			}
		}
	};
};