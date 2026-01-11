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

using Game.Application.Common.Models;
using Game.Application.Common.Models.Interfaces;
using Game.Application.Common.Models.ValueObjects;
using Game.Application.Configuration.Enums;
using Game.Application.Configuration.Registries;
using Game.Domain.Configuration;
using Game.Infrastructure.Configuration.Interfaces;
using Nomad.Core;
using Nomad.Core.Exceptions;
using Nomad.CVars;
using System;
using System.Runtime.CompilerServices;

namespace Game.Application.Configuration.Services {
	/*
	===================================================================================
	
	SettingsService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	internal sealed class DisplaySettingsService : IDisplaySettingsService {
		private readonly record struct DisplayConfiguration( DisplaySettingsService owner ) : IDisplayConfiguration {
			private readonly DisplaySettingsService _owner = owner;

			public int Monitor {
				get => _owner._config.Monitor;
				set => _owner._config = _owner._config with { Monitor = value };
			}
			public int DRSTargetFrames {
				get => _owner._config.DRSTargetFrames;
				set => _owner._config = _owner._config with { DRSTargetFrames = value };
			}
			public float Brightness {
				get => _owner._config.Brightness;
				set => _owner._config = _owner._config with { Brightness = value };
			}
			public MaxFps MaxFps {
				get => _owner._config.MaxFps;
				set => _owner._config = _owner._config with { MaxFps = value };
			}
			public WindowMode WindowMode {
				get => _owner._config.WindowMode;
				set => _owner._config = _owner._config with { WindowMode = value };
			}
			public WindowResolution WindowResolution {
				get => _owner._config.WindowResolution;
				set => _owner._config = _owner._config with { WindowResolution = value };
			}
			public AspectRatio AspectRatio {
				get => _owner._config.AspectRatio;
				set => _owner._config = _owner._config with { AspectRatio = value };
			}
			public VSyncMode VSyncMode {
				get => _owner._config.VSyncMode;
				set => _owner._config = _owner._config with { VSyncMode = value };
			}
			public AntiAliasing AntiAliasing {
				get => _owner._config.AntiAliasing;
				set => _owner._config = _owner._config with { AntiAliasing = value };
			}
			public PerformanceOverlayPreset PerformanceOverlayPreset {
				get => _owner._config.PerformanceOverlayPreset;
				set => _owner._config = _owner._config with { PerformanceOverlayPreset = value };
			}
			public bool SeparateRenderingThread {
				get => _owner._config.SeparateRenderingThread;
				set => _owner._config = _owner._config with { SeparateRenderingThread = value };
			}
		};

		public IDisplayConfiguration Config => _displayConfig;
		private readonly DisplayConfiguration _displayConfig;

		public int MonitorCount => _systemConfig.MonitorCount;

		public WindowSize NativeResolution => _nativeResolution;
		private WindowSize _nativeResolution;

		private DisplayConfig _config;

		private IDisplayConfig _systemConfig;
		private readonly ICVarSystemService _cvarSystem;

		/*
		===============
		DisplaySettingsService
		===============
		*/
		/// <summary>
		/// Creates a <see cref="DisplaySettingsService"/>
		/// </summary>
		/// <param name="cvarSystem"></param>
		/// <param name="displayConfig"></param>
		public DisplaySettingsService( ICVarSystemService cvarSystem, IDisplayConfig displayConfig ) {
			ArgumentNullException.ThrowIfNull( cvarSystem );
			ArgumentNullException.ThrowIfNull( displayConfig );

			DisplayCVars.Register( cvarSystem );

			_displayConfig = new DisplayConfiguration( this );

			_systemConfig = displayConfig;
			_systemConfig.InitConfig();

			_systemConfig.GetNativeResolutionForMonitor( _systemConfig.Monitor, out _nativeResolution );

			_cvarSystem = cvarSystem;

			InitConfiguration();
		}

		/*
		===============
		InitConfiguration
		===============
		*/
		private void InitConfiguration() {
			var monitor = GetCVar<int>( Constants.CVars.Display.MONITOR );
			monitor.ValueChanged.Subscribe( this, OnMonitorChanged );

			var drsTargetFrames = GetCVar<int>( Constants.CVars.Display.DRS_TARGET_FRAMES );

			var brightness = GetCVar<float>( Constants.CVars.Display.BRIGHTNESS );
			brightness.ValueChanged.Subscribe( this, OnBrightnessChanged );

			var maxFps = GetCVar<MaxFps>( Constants.CVars.Display.MAX_FPS );
			maxFps.ValueChanged.Subscribe( this, OnMaxFpsChanged );

			var windowMode = GetCVar<WindowMode>( Constants.CVars.Display.WINDOW_MODE );
			windowMode.ValueChanged.Subscribe( this, OnWindowModeChanged );

			var windowResolution = GetCVar<WindowResolution>( Constants.CVars.Display.WINDOW_RESOLUTION );
			windowResolution.ValueChanged.Subscribe( this, OnWindowResolutionChanged );

			var vsyncMode = GetCVar<VSyncMode>( Constants.CVars.Display.VSYNC_MODE );
			vsyncMode.ValueChanged.Subscribe( this, OnVSyncModeChanged );

			var aspectRatio = GetCVar<AspectRatio>( Constants.CVars.Display.ASPECT_RATIO );
			aspectRatio.ValueChanged.Subscribe( this, OnAspectRatioChanged );

			var antiAliasing = GetCVar<AntiAliasing>( Constants.CVars.Display.ANTI_ALIASING );
			antiAliasing.ValueChanged.Subscribe( this, OnAntiAliasingChanged );

			_config = new DisplayConfig {
				Monitor = monitor.Value,
				DRSTargetFrames = drsTargetFrames.Value,
				Brightness = brightness.Value,
				MaxFps = maxFps.Value,
				WindowMode = windowMode.Value,
				WindowResolution = windowResolution.Value,
				AspectRatio = aspectRatio.Value,
				VSyncMode = vsyncMode.Value,
				AntiAliasing = antiAliasing.Value,
				PerformanceOverlayPreset = GetCVar<PerformanceOverlayPreset>( "display.PerformanceOverlay" ).Value,
				SeparateRenderingThread = GetCVar<bool>( Constants.CVars.Display.SEPARATE_RENDERING_THREAD ).Value
			};
		}

		/*
		===============
		SetConfig
		===============
		*/
		public void SetConfig( DisplayConfig config ) {
			_config = config;
		}

		/*
		===============
		GetSupportedAntiAliasingModes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] GetSupportedAntiAliasingModes() {
			var supportedModes = new string[ (int)AntiAliasing.Count ];

			for ( int i = 0; i < supportedModes.Length; i++ ) {
				supportedModes[ i ] = ( (AntiAliasing)( (int)AntiAliasing.None + i ) ).AsString();
			}

			return supportedModes;
		}

		/*
		===============
		GetSupportedVSyncModes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] GetSupportedVSyncModes() {
			var supportedModes = new string[ (int)VSyncMode.Count ];

			for ( int i = 0; i < supportedModes.Length; i++ ) {
				supportedModes[ i ] = ( (VSyncMode)( (int)VSyncMode.Off + i ) ).AsString();
			}

			return supportedModes;
		}

		/*
		===============
		GetSupportedDisplayModes
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] GetSupportedDisplayModes() {
			var supportedModes = new string[ (int)WindowMode.Count ];

			for ( int i = 0; i < supportedModes.Length; i++ ) {
				supportedModes[ i ] = ( (WindowMode)( (int)WindowMode.Windowed + i ) ).AsString();
			}

			return supportedModes;
		}

		/*
		===============
		GetSupportedResolutions
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] GetSupportedResolutions() {
			var resolutions = _systemConfig.GetSupportedResolutions( _config.Monitor );
			var supportedResolutions = new string[ resolutions.Length ];

			for ( int i = 0; i < supportedResolutions.Length; i++ ) {
				supportedResolutions[ i ] = resolutions[ i ].ToDisplayString();
			}

			return supportedResolutions;
		}

		/*
		===============
		GetSupportedAspectRatios
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] GetSupportedAspectRatios() {
			var supportedRatios = new string[ (int)AspectRatio.Count ];

			for ( int i = 0; i < supportedRatios.Length; i++ ) {
				supportedRatios[ i ] = ( (AspectRatio)( (uint)AspectRatio.Aspect_Automatic + i ) ).ToDisplayString();
			}

			return supportedRatios;
		}

		/*
		===============
		GetSupportedFrameLimits
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string[] GetSupportedFrameLimits() {
			var supportedFrameLimits = new string[ (int)MaxFps.Count ];

			for ( int i = 0; i < supportedFrameLimits.Length; i++ ) {
				supportedFrameLimits[ i ] = ( (MaxFps)( (uint)MaxFps.MaxFps30 + i ) ).ToDisplayString();
			}

			return supportedFrameLimits;
		}

		/*
		===============
		OnMonitorChanged
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnMonitorChanged( in CVarValueChangedEventArgs<int> args )
			=> _config = _config with { Monitor = args.NewValue };
		
		/*
		===============
		OnWindowModeChanged
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnWindowModeChanged( in CVarValueChangedEventArgs<WindowMode> args )
			=> _config = _config with { WindowMode = args.NewValue };
		
		/*
		===============
		OnWindowResolutionChanged
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnWindowResolutionChanged( in CVarValueChangedEventArgs<WindowResolution> args )
			=> _config = _config with { WindowResolution = args.NewValue };

		/*
		===============
		OnVSyncModeChanged
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnVSyncModeChanged( in CVarValueChangedEventArgs<VSyncMode> args )
			=> _config = _config with { VSyncMode = args.NewValue };
		
		/*
		===============
		OnAntiAliasingChanged
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnAntiAliasingChanged( in CVarValueChangedEventArgs<AntiAliasing> args )
			=> _config = _config with { AntiAliasing = args.NewValue };
		
		/*
		===============
		OnBrightnessChanged
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnBrightnessChanged( in CVarValueChangedEventArgs<float> args )
			=> _config = _config with { Brightness = args.NewValue };
		
		/*
		===============
		OnAspectRatioChanged
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnAspectRatioChanged( in CVarValueChangedEventArgs<AspectRatio> args )
			=> _config = _config with { AspectRatio = args.NewValue };
		
		/*
		===============
		OnMaxFpsChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private void OnMaxFpsChanged( in CVarValueChangedEventArgs<MaxFps> args )
			=> _config = _config with { MaxFps = args.NewValue };

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private ICVar<T> GetCVar<T>( string name ) =>
			_cvarSystem.GetCVar<T>( name ) ?? throw new CVarMissing( name );
	};
};