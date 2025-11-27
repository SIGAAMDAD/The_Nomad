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
using NomadCore.Interfaces.ConsoleSystem;
using NomadCore.Abstractions.Services;
using Game.Infrastructure.Configuration.Interfaces;
using Game.Application.Configuration.Enums;

namespace Game.Infrastructure.Configuration.Godot {
	/*
	===================================================================================
	
	GodotDisplay
	
	===================================================================================
	*/
	/// <summary>
	/// Handles updating godot display settings and initializes cvars.
	/// </summary>

	public sealed class GodotDisplay : IDisplayConfig {
		private readonly record struct VSyncModeData {
			public readonly DisplayServer.VSyncMode VSyncMode { get; init; }
			public readonly int SwapChainImageCount { get; init; }
		};

		private enum ThreadModel : int {
			Unsafe,
			Safe,
			Separate
		};

		private readonly Dictionary<VSyncMode, VSyncModeData> VSyncModes = new Dictionary<VSyncMode, VSyncModeData>() {
			{ VSyncMode.Off, new VSyncModeData { SwapChainImageCount = 2, VSyncMode = DisplayServer.VSyncMode.Disabled } },
			{ VSyncMode.On, new VSyncModeData { SwapChainImageCount = 2, VSyncMode = DisplayServer.VSyncMode.Enabled } },
			{ VSyncMode.Adaptive, new VSyncModeData { SwapChainImageCount = 2, VSyncMode = DisplayServer.VSyncMode.Adaptive } },
			{ VSyncMode.TripleBuffered, new VSyncModeData { SwapChainImageCount = 3, VSyncMode = DisplayServer.VSyncMode.Disabled } }
		};

		/// <summary>
		/// Various premade window resolutions, sourced from wikipedia
		/// </summary>
		private readonly Dictionary<WindowResolution, Vector2I> WindowResolutions = new Dictionary<WindowResolution, Vector2I>() {
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

		private readonly float[] AspectRatios = [
			0.0f, // automatic
			(float)( 4.0f / 3.0f ),
			(float)( 16.0f / 10.0f ),
			(float)( 16.0f / 9.0f ),
			(float)( 21.0f / 9.0f )
		];

		public int NativeDisplayWidth => NativeScreenSize.X;
		public int NativeDisplayHeight => NativeScreenSize.Y;

		public int Monitor => _screenIndex;
		private int _screenIndex;

		public int MonitorCount => ScreenSizes.Length;

		public int DisplayWidth => _displayWidth;
		private int _displayWidth;

		public int DisplayHeight => _displayHeight;
		private int _displayHeight;

		public float RefreshRate => _refreshRate;
		private float _refreshRate;

		private readonly ICVarSystemService Service;
		private readonly ILoggerService? Logger;
		private readonly Vector2I NativeScreenSize;
		private readonly Vector2I[] ScreenSizes;

		/*
		===============
		GodotDisplay
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="service"></param>
		/// <param name="logger"></param>
		public GodotDisplay( ICVarSystemService? service, ILoggerService? logger ) {
			ArgumentNullException.ThrowIfNull( service );
			ArgumentNullException.ThrowIfNull( logger );

			Service = service;
			Logger = logger;

			NativeScreenSize = DisplayServer.ScreenGetSize( DisplayServer.WindowGetCurrentScreen() );
			ScreenSizes = GetScreenList();

			service.GetCVar<WindowMode>( "display.WindowMode" )?.ValueChanged.Subscribe( this, OnWindowModeChanged );
			service.GetCVar<WindowResolution>( "display.WindowResolution" )?.ValueChanged.Subscribe( this, OnWindowResolutionChanged );
			service.GetCVar<int>( "display.Monitor" )?.ValueChanged.Subscribe( this, OnMonitorChanged );
			service.GetCVar<AntiAliasing>( "display.AntiAliasing" )?.ValueChanged.Subscribe( this, OnAntiAliasingChanged );
			service.GetCVar<AspectRatio>( "display.AspectRatio" )?.ValueChanged.Subscribe( this, OnAspectRatioChanged );
			service.GetCVar<PerformanceOverlayPreset>( "display.PerformanceOverlay" )?.ValueChanged.Subscribe( this, OnPerformanceOverlayChanged );
			service.GetCVar<bool>( "r.SeparateRenderingThread" )?.ValueChanged.Subscribe( this, OnSeparateRenderingThreadChanged );
			service.GetCVar<VSyncMode>( "display.VSyncMode" )?.ValueChanged.Subscribe( this, OnVSyncModeChanged );
		}

		/*
		===============
		GetNativeResolutionForMonitor
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="monitorIndex"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void GetNativeResolutionForMonitor( int monitorIndex, out int nativeWidth, out int nativeHeight ) {
			if ( monitorIndex < 0 || monitorIndex >= ScreenSizes.Length ) {
				throw new ArgumentOutOfRangeException( nameof( monitorIndex ) );
			}
			Vector2I screenSize = ScreenSizes[ monitorIndex ];
			nativeWidth = screenSize.X;
			nativeHeight = screenSize.Y;
		}

		/*
		===============
		UpdateDisplayData
		===============
		*/
		/// <summary>
		/// Updates the current monitor/display data
		/// </summary>
		/// <param name="screenIndex">The monitor index to get the data from.</param>
		private void UpdateDisplayData( int screenIndex ) {
			_refreshRate = DisplayServer.ScreenGetRefreshRate( screenIndex );

			Vector2I screenSize = ScreenSizes[ screenIndex ];
			_displayWidth = screenSize.X;
			_displayHeight = screenSize.Y;
		}

		/*
		===============
		GetScreenList
		===============
		*/
		/// <summary>
		/// Gets a list of all the currently available monitors and fetches their sizes and other various data.
		/// </summary>
		/// <returns></returns>
		private Vector2I[] GetScreenList() {
			int screenCount = DisplayServer.GetScreenCount();
			Vector2I[] sizes = new Vector2I[ screenCount ];

			for ( int i = 0; i < screenCount; i++ ) {
				sizes[ i ] = DisplayServer.ScreenGetSize( i );
			}

			return sizes;
		}

		/*
		===============
		OnWindowModeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWindowModeChanged( in ICVarValueChangedEventData<WindowMode> args ) {

		}

		/*
		===============
		OnWindowResolutionChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWindowResolutionChanged( in ICVarValueChangedEventData<WindowResolution> args ) {
			Vector2I resolution;
			if ( args.Value == WindowResolution.Res_Native ) {
				resolution = NativeScreenSize;
			} else if ( !WindowResolutions.TryGetValue( args.Value, out resolution ) ) {
				Logger?.PrintError( $"Display.OnWindowResolutionChanged: invalid WindowResolution '{args.Value}', setting to default - '{WindowResolution.Default}'" );
				var windowResolution = Service.GetCVar<WindowResolution>( "display.WindowResolution" );
				windowResolution.Reset();
				resolution = WindowResolutions[ windowResolution.Value ];
			}
			DisplayServer.WindowSetSize( resolution );
		}

		/*
		===============
		OnMonitorChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMonitorChanged( in ICVarValueChangedEventData<int> args ) {
			DisplayServer.WindowSetCurrentScreen( args.Value );
			_screenIndex = args.Value;

			UpdateDisplayData( _screenIndex );
		}

		/*
		===============
		OnAntiAliasingChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnAntiAliasingChanged( in ICVarValueChangedEventData<AntiAliasing> args ) {
		}

		/*
		===============
		OnAspectRatioChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnAspectRatioChanged( in ICVarValueChangedEventData<AspectRatio> args ) {
		}

		/*
		===============
		OnPerformanceOverlayChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnPerformanceOverlayChanged( in ICVarValueChangedEventData<PerformanceOverlayPreset> args ) {
		}

		/*
		===============
		OnSeparateRenderingThreadChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnSeparateRenderingThreadChanged( in ICVarValueChangedEventData<bool> args ) {
			ProjectSettings.SetSetting( "rendering/driver/threads/thread_model", (int)( args.Value ? ThreadModel.Separate : ThreadModel.Safe ) );
		}

		/*
		===============
		OnVSyncModeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnVSyncModeChanged( in ICVarValueChangedEventData<VSyncMode> args ) {
			if ( !VSyncModes.TryGetValue( args.Value, out VSyncModeData data ) ) {
				Logger?.PrintError( $"Display.OnVSyncModeChanged: invalid VSyncMode '{args.Value}', setting to default - '{VSyncMode.Default}'" );
				var vsyncMode = Service.GetCVar<VSyncMode>( "display.VSyncMode" );
				vsyncMode.Reset();
				data = VSyncModes[ VSyncMode.Default ];
			}
			DisplayServer.WindowSetVsyncMode( data.VSyncMode );
			ProjectSettings.SetSetting( "rendering/rendering_device/vsync/swap_chain_image_count", data.SwapChainImageCount );
		}
	};
};