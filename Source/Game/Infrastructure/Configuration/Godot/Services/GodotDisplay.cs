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
using Game.Infrastructure.Configuration.Interfaces;
using Game.Application.Configuration.Enums;
using System.Runtime.CompilerServices;
using Game.Application.Common.Models.ValueObjects;
using Game.Infrastructure.Configuration.Godot.ValueObjects;
using Nomad.Core.Exceptions;
using Nomad.CVars;
using Nomad.Core.Logger;
using Nomad.Core;

namespace Game.Infrastructure.Configuration.Godot.Services {
	/*
	===================================================================================
	
	GodotDisplay
	
	===================================================================================
	*/
	/// <summary>
	/// Handles updating godot display settings and initializes cvars.
	/// </summary>

	internal sealed class GodotDisplay : IDisplayConfig {
		private static readonly StringName BrightnessAmountShaderName = "brightness_amount";

		public WindowSize NativeDisplaySize => _nativeScreenSize;

		public int Monitor => _screenIndex;
		private int _screenIndex;

		public int MonitorCount => _monitors.Length;

		public WindowSize DisplaySize => new( _displayWidth, _displayHeight );
		private int _displayWidth;
		private int _displayHeight;

		public float RefreshRate => _refreshRate;
		private float _refreshRate;

		private readonly ICVarSystemService _cvarSystem;
		private readonly ILoggerService _logger;
		private readonly WindowSize _nativeScreenSize;
		private readonly Monitor[] _monitors;

		private readonly Rid _viewportRid;

		/*
		===============
		GodotDisplay
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="viewportRid"></param>
		/// <param name="cvarSystem"></param>
		/// <param name="logger"></param>
		/// <exception cref="ArgumentException"></exception>
		public GodotDisplay( Rid viewportRid, ICVarSystemService cvarSystem, ILoggerService logger ) {
			ArgumentNullException.ThrowIfNull( cvarSystem );
			ArgumentNullException.ThrowIfNull( logger );
			if ( !viewportRid.IsValid ) {
				throw new ArgumentException( "Invalid viewportRid!" );
			}

			_cvarSystem = cvarSystem;
			_logger = logger;
			_viewportRid = viewportRid;

			var screenSize = DisplayServer.ScreenGetSize( DisplayServer.WindowGetCurrentScreen() );
			_nativeScreenSize = new WindowSize( screenSize.X, screenSize.Y );
			_monitors = GetScreenList();

			_screenIndex = DisplayServer.GetPrimaryScreen();

			var minSize = (WindowSize)WindowResolution.Min;
			DisplayServer.WindowSetMinSize( new Vector2I( minSize.Width, minSize.Height ) );

			var maxSize = (WindowSize)WindowResolution.Max;
			DisplayServer.WindowSetMaxSize( new Vector2I( maxSize.Width, maxSize.Height ) );
		}

		/*
		===============
		InitConfig
		===============
		*/
		/// <summary>
		/// Initializes the display configuration.
		/// </summary>
		public void InitConfig() {
			var windowMode = GetCVar<WindowMode>( Constants.CVars.Display.WINDOW_MODE );
			windowMode.ValueChanged.Subscribe( this, OnWindowModeChanged );
			SetWindowMode( windowMode.Value );

			var windowResolution = GetCVar<WindowResolution>( Constants.CVars.Display.WINDOW_RESOLUTION );
			windowResolution.ValueChanged.Subscribe( this, OnWindowResolutionChanged );
			SetWindowResolution( windowResolution.Value );

			var monitor = GetCVar<int>( Constants.CVars.Display.MONITOR );
			monitor.ValueChanged.Subscribe( this, OnMonitorChanged );
			SetMonitor( monitor.Value );

			var antiAliasing = GetCVar<AntiAliasing>( Constants.CVars.Display.ANTI_ALIASING );
			antiAliasing.ValueChanged.Subscribe( this, OnAntiAliasingChanged );
			SetAntiAliasingMethod( _viewportRid, antiAliasing.Value );

			var aspectRatio = GetCVar<AspectRatio>( Constants.CVars.Display.ASPECT_RATIO );
			aspectRatio.ValueChanged.Subscribe( this, OnAspectRatioChanged );
			SetAspectRatio( aspectRatio.Value );

			GetCVar<PerformanceOverlayPreset>( "display.PerformanceOverlay" ).ValueChanged.Subscribe( this, OnPerformanceOverlayChanged );

			var separateRenderingThread = GetCVar<bool>( Constants.CVars.Display.SEPARATE_RENDERING_THREAD );
			separateRenderingThread.ValueChanged.Subscribe( this, OnSeparateRenderingThreadChanged );
			SetRenderingThreadModel( (int)( separateRenderingThread.Value ? ThreadModel.Separate : ThreadModel.Safe ) );

			var vsyncMode = GetCVar<VSyncMode>( Constants.CVars.Display.VSYNC_MODE );
			vsyncMode.ValueChanged.Subscribe( this, OnVSyncModeChanged );
			SetVSyncMode( vsyncMode.Value );

			var maxFps = GetCVar<MaxFps>( Constants.CVars.Display.MAX_FPS );
			maxFps.ValueChanged.Subscribe( this, OnMaxFpsChanged );
			SetMaxFps( maxFps.Value );

			var brightness = GetCVar<float>( Constants.CVars.Display.BRIGHTNESS );
			brightness.ValueChanged.Subscribe( this, OnBrightnessChanged );
			SetBrightnessValue( brightness.Value );
		}

		/*
		===============
		GetSupportedResolutions
		===============
		*/
		/// <summary>
		/// Returns a list of monitor supported window resolutions.
		/// </summary>
		/// <param name="monitorIndex"></param>
		/// <returns></returns>
		public WindowResolution[] GetSupportedResolutions( int monitorIndex ) {
			var monitor = _monitors[ monitorIndex ];
			var resolutions = new List<WindowResolution>();

			for ( var resolution = WindowResolution.Res_640x480; resolution < WindowResolution.Count; resolution++ ) {
				if ( monitor.ScreenSize >= resolution ) {
					resolutions.Add( resolution );
				}
			}

			return [ .. resolutions ];
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
		public void GetNativeResolutionForMonitor( int monitorIndex, out WindowSize nativeSize ) {
			if ( monitorIndex < 0 || monitorIndex >= _monitors.Length ) {
				throw new ArgumentOutOfRangeException( nameof( monitorIndex ) );
			}
			nativeSize = _monitors[ monitorIndex ].ScreenSize;
		}

		/*
		===============
		UpdateDisplayData
		===============
		*/
		/// <summary>
		/// Updates the current monitor/display data.
		/// </summary>
		/// <param name="screenIndex">The monitor index to get the data from.</param>
		private void UpdateDisplayData( int screenIndex ) {
			_refreshRate = DisplayServer.ScreenGetRefreshRate( screenIndex );

			WindowSize screenSize = _monitors[ screenIndex ].ScreenSize;
			_displayWidth = screenSize.Width;
			_displayHeight = screenSize.Height;
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
		private static Monitor[] GetScreenList() {
			int screenCount = DisplayServer.GetScreenCount();
			Monitor[] screens = new Monitor[ screenCount ];

			for ( int i = 0; i < screenCount; i++ ) {
				screens[ i ] = new Monitor( i );
			}

			return screens;
		}

		/*
		===============
		SetBrightnessValue
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void SetBrightnessValue( float value )
			=> RenderingServer.GlobalShaderParameterSet( BrightnessAmountShaderName, value );

		/*
		===============
		SetWindowMode
		===============
		*/
		private static void SetWindowMode( WindowModeData data ) {
			DisplayServer.WindowSetMode( data.Mode );
			DisplayServer.WindowSetExclusive( (int)DisplayServer.MainWindowId, data.Exclusive );
			DisplayServer.WindowSetFlag( DisplayServer.WindowFlags.Borderless, data.Borderless );
		}

		/*
		===============
		SetWindowResolution
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void SetWindowResolution( WindowSize resolution )
			=> DisplayServer.WindowSetSize( new Vector2I( resolution.Width, resolution.Height ) );
	
		/*
		===============
		SetMonitor
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void SetMonitor( int monitorIndex )
			=> DisplayServer.WindowSetCurrentScreen( monitorIndex );

		/*
		===============
		SetVSyncMode
		===============
		*/
		private static void SetVSyncMode( VSyncData data ) {
			DisplayServer.WindowSetVsyncMode( data.Mode );
			ProjectSettings.SetSetting( "rendering/rendering_device/vsync/swap_chain_image_count", data.SwapChainImageCount );
		}

		/*
		===============
		SetAntiAliasingMethod
		===============
		*/
		private static void SetAntiAliasingMethod( Rid viewportRid, AntiAliasingData data ) {
			RenderingServer.ViewportSetMsaa2D( viewportRid, data.Msaa );
			RenderingServer.ViewportSetScreenSpaceAA( viewportRid, data.ScreenSpaceAA );
			RenderingServer.ViewportSetUseTaa( viewportRid, data.UseTaa );
		}

		/*
		===============
		SetAspectRatio
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void SetAspectRatio( AspectRatioValue data )
			=> ProjectSettings.SetSetting( "display/window/stretch/scale", data.Ratio );
		
		/*
		===============
		SetRenderingThreadModel
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void SetRenderingThreadModel( int threadModel )
			=> ProjectSettings.SetSetting( "rendering/driver/threads/thread_model", threadModel );
		
		/*
		===============
		SetMaxFps
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void SetMaxFps( MaxFps maxFps )
			=> Engine.MaxFps = maxFps.ToInt();

		/*
		===============
		OnWindowModeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWindowModeChanged( in CVarValueChangedEventArgs<WindowMode> args ) {
			WindowModeData data;
			try {
				data = args.NewValue;
			} catch ( ArgumentOutOfRangeException ) {
				_logger?.PrintError( $"GodotDisplay.OnWindowModeChanged: invalid WindowMode '{args.NewValue}', setting to default - '{WindowMode.Default}'" );
				var windowMode = GetCVar<WindowMode>( "display.WindowMode" );
				windowMode.Reset();
				data = windowMode.Value;
			}

			_logger?.PrintLine( $"GodotDisplay.OnWindowModeChanged: settings WindowMode to '{args.NewValue}'..." );
			SetWindowMode( data );
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
		private void OnWindowResolutionChanged( in CVarValueChangedEventArgs<WindowResolution> args ) {
			WindowSize resolution;
			if ( args.NewValue == WindowResolution.Res_Native ) {
				resolution = _nativeScreenSize;
			} else {
				try {
					resolution = args.NewValue;
				} catch ( ArgumentOutOfRangeException ) {
					_logger?.PrintError( $"GodotDisplay.OnWindowResolutionChanged: invalid WindowResolution '{args.NewValue}', setting to default - '{WindowResolution.Default}'" );
					var windowResolution = GetCVar<WindowResolution>( "display.WindowResolution" );
					windowResolution.Reset();
					resolution = windowResolution.Value;
				}
			}

			_logger?.PrintLine( $"GodotDisplay.OnWindowResolutionChanged: settings WindowResolution to '{args.NewValue}'..." );
			SetWindowResolution( resolution );
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
		private void OnMonitorChanged( in CVarValueChangedEventArgs<int> args ) {
			_screenIndex = args.NewValue;
			SetMonitor( _screenIndex );
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
		private void OnAntiAliasingChanged( in CVarValueChangedEventArgs<AntiAliasing> args ) {
			AntiAliasingData data;
			try {
				data = args.NewValue;
			} catch ( ArgumentOutOfRangeException ) {
				_logger?.PrintError( $"GodotDisplay.OnAntiAliasingChanged: invalid AntiAliasing '{args.NewValue}', setting to default - '{AntiAliasing.Default}'" );
				var antiAliasing = GetCVar<AntiAliasing>( "display.AntiAliasing" );
				antiAliasing.Reset();
				data = antiAliasing.Value;
			}

			_logger?.PrintLine( $"GodotDisplay.OnAntiAliasingChanged: settings AntiAliasing to '{args.NewValue}'..." );
			SetAntiAliasingMethod( _viewportRid, data );
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
		private void OnAspectRatioChanged( in CVarValueChangedEventArgs<AspectRatio> args ) {
			AspectRatioValue data;
			try {
				data = args.NewValue;
			} catch ( ArgumentOutOfRangeException ) {
				_logger?.PrintError( $"GodotDisplay.OnAspectRatioChanged: invalid AspectRatio '{args.NewValue}', setting to default - '{AntiAliasing.Default}'" );
				var aspectRatio = GetCVar<AspectRatio>( "display.AspectRatio" );
				aspectRatio.Reset();
				data = aspectRatio.Value;
			}

			_logger?.PrintLine( $"GodotDisplay.OnAspectRatioChanged: settings AspectRatio to '{args.NewValue}'..." );
			SetAspectRatio( data );
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
		private void OnPerformanceOverlayChanged( in CVarValueChangedEventArgs<PerformanceOverlayPreset> args ) {
		}

		/*
		===============
		OnSeparateRenderingThreadChanged
		===============
		*/
		/// <summary>
		/// Updates godot's rendering thread status ("rendering/driver/threads/thread_model" setting).
		/// </summary>
		/// <param name="args"></param>
		private void OnSeparateRenderingThreadChanged( in CVarValueChangedEventArgs<bool> args ) {
			_logger?.PrintLine( $"GodotDisplay.OnSeparateRenderingThreadChanged: setting rendering thread status to '{args.NewValue}'..." );
			SetRenderingThreadModel( (int)( args.NewValue ? ThreadModel.Separate : ThreadModel.Safe ) );
		}

		/*
		===============
		OnVSyncModeChanged
		===============
		*/
		/// <summary>
		/// Sets godot's internal vsync mode.
		/// </summary>
		/// <param name="args"></param>
		private void OnVSyncModeChanged( in CVarValueChangedEventArgs<VSyncMode> args ) {
			VSyncData data;
			try {
				data = args.NewValue;
			} catch ( ArgumentOutOfRangeException ) {
				_logger?.PrintError( $"GodotDisplay.OnVSyncModeChanged: invalid VSyncMode '{args.NewValue}', setting to default - '{VSyncMode.Default}'" );
				var vsyncMode = GetCVar<VSyncMode>( "display.VSyncMode" );
				vsyncMode.Reset();
				data = vsyncMode.Value;
			}

			_logger?.PrintLine( $"GodotDisplay.OnVSyncModeChanged: settings VSyncMode to '{args.NewValue}'..." );
			SetVSyncMode( data );
		}

		/*
		===============
		OnBrightnessChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnBrightnessChanged( in CVarValueChangedEventArgs<float> args ) {
			_logger?.PrintLine( $"GodotDisplay.OnBrightnessChanged: setting Brightness to '{args.NewValue}'..." );
			SetBrightnessValue( args.NewValue );
		}

		/*
		===============
		OnMaxFpsChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMaxFpsChanged( in CVarValueChangedEventArgs<MaxFps> args ) {
			_logger?.PrintLine( $"GodotDisplay.OnMaxFpsChanged: setting MaxFps to '{args.NewValue}'..." );
			SetMaxFps( args.NewValue );
		}

		/*
		===============
		GetCVar
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private ICVar<T> GetCVar<T>( string name ) =>
			_cvarSystem.GetCVar<T>( name ) ?? throw new CVarMissing( name );
	};
};