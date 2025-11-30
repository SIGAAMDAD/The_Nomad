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

using Game.Application.Common.Interfaces;
using Game.Application.Common.Models;
using Game.Application.Configuration.Enums;
using Game.Infrastructure.Configuration.Interfaces;
using NomadCore.Abstractions.Services;
using System;
using System.Linq;

namespace Game.Application.Configuration.Services {
	/*
	===================================================================================
	
	DisplaySettingsService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class DisplaySettingsService : IDisplaySettingsService {
		private class DisplayConfiguration( DisplaySettingsService service ) {
			private readonly DisplaySettingsService _service = service;

			public int Monitor {
				get => _service._config.Monitor;
			}
		};

		public IDisplayConfiguration Config;

		private DisplayConfig _config;

		private readonly ICVarSystemService _cvarSystem;
		private readonly IDisplayConfig _displayConfig;

		/*
		===============
		DisplaySettingsService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		/// <param name="displayConfig"></param>
		public DisplaySettingsService( ICVarSystemService? cvarSystem, IDisplayConfig displayConfig ) {
			ArgumentNullException.ThrowIfNull( cvarSystem );
			ArgumentNullException.ThrowIfNull( displayConfig );

			_cvarSystem = cvarSystem;
			_displayConfig = displayConfig;

			_config = new DisplayConfig() {
				Monitor = _cvarSystem.GetCVar<int>( "display.Monitor" ).Value,
				WindowMode = _cvarSystem.GetCVar<WindowMode>( "display.WindowMode" ).Value,
				WindowResolution = _cvarSystem.GetCVar<WindowResolution>( "display.WindowResolution" ).Value,
				VSyncMode = _cvarSystem.GetCVar<VSyncMode>( "display.VSyncMode" ).Value,
				MaxFps = _cvarSystem.GetCVar<MaxFps>( "display.MaxFps" ).Value,
				AntiAliasing = _cvarSystem.GetCVar<AntiAliasing>( "display.AntiAliasing" ).Value,
				AspectRatio = _cvarSystem.GetCVar<AspectRatio>( "display.AspectRatio" ).Value,
				PerformanceOverlayPreset = _cvarSystem.GetCVar<PerformanceOverlayPreset>( "display.PerformanceOverlayPreset" ).Value,
				SeparateRenderingThread = _cvarSystem.GetCVar<bool>( "r.SeparateRenderingThread" ).Value
			};
		}

		/*
		===============
		GetCurrentConfig
		===============
		*/
		public DisplayConfig GetCurrentConfig() {
			return _config;
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
			var supportedResolutions = _displayConfig.GetSupportedResolutions( _displayConfig.Monitor );
			return [ .. supportedResolutions.Select( s => s.ToDisplayString() ) ];
		}

		/*
		===============
		SetWindowResolution
		===============
		*/
		public void SetWindowResolution( WindowResolution resolution ) {
			_config = _config with { WindowResolution = resolution };
		}

		public void SetWindowMode( WindowMode mode ) {
			_config = _config with { WindowMode = mode };
		}

		public void SetVSyncMode( VSyncMode mode ) {
			_config = _config with { VSyncMode = mode };
		}

		public void SetSeparateRenderingThread( bool value ) {
			_config = _config with { SeparateRenderingThread = value };
		}

		public void SetMonitor( int monitor ) {
			_config = _config with { Monitor = monitor };
		}

		public void SetDRSTargetFrames( int targetFrames ) {
			_config = _config with { DRSTargetFrames = targetFrames };
		}

		public void SetAntiAliasing( AntiAliasing antiAliasing ) {
			_config = _config with { AntiAliasing = antiAliasing };
		}

		public void SetAspectRatio( AspectRatio aspectRatio ) {
			_config = _config with { AspectRatio = aspectRatio };
		}
	};
};