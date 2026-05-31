/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Collections.Generic;
using Nomad.Core.Engine.Rendering;
using Nomad.Core.Engine.Services;
using Nomad.Core.Engine.Windowing;
using Nomad.EngineUtils.Settings.Services;
using Nomad.Game.Presentation.Screens.SettingsMenu;

namespace Nomad.Game.Presentation.Screens.SettingsMenu
{
	/*
	===================================================================================

	DisplaySettingsContainerModel

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	internal sealed class DisplaySettingsContainerModel
	{
		public WindowMode WindowMode => _service.Config.WindowMode;
		public WindowResolution WindowResolution => _service.Config.Resolution;
		public int MonitorIndex => _service.Config.MonitorIndex;

		public AntiAliasingMode AntiAliasing => _service.Config.AntiAliasing;

		public VSyncMode VSyncMode => _service.Config.VSyncMode;

		public int MaximumFramerate => _service.Config.MaximumFrameRate;
		public int MaximumFramerateMin { get; private set; } = 30;
		public int MaximumFramerateMax { get; private set; } = 600;

		public IReadOnlyList<string> WindowModes => _windowModes;
		private readonly string[] _windowModes;

		public IReadOnlyList<string> WindowResolutions => _resolutions;
		private readonly string[] _resolutions;

		public IReadOnlyList<string> VSyncModes => _vsyncModes;
		private readonly string[] _vsyncModes;

		public IReadOnlyList<string> Monitors => _monitors;
		private readonly string[] _monitors;

		private readonly DisplaySettingsService _service;

		/*
		===============
		DisplaySettingsContainerModel
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="windowService"></param>
		/// <param name="service"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public DisplaySettingsContainerModel( IWindowService windowService, DisplaySettingsService service )
		{
			_service = service ?? throw new ArgumentNullException( nameof( service ) );
			_windowModes = new string[] {
				"Windowed",
				"Borderless Windowed",
				"Fullscreen",
				"Borderless Fullscreen",
				"Exclusive Fullscreen"
			};

			var supportedResolutions = windowService.GetSupportedResolutions( _service.Config.MonitorIndex );
			_resolutions = new string[supportedResolutions.Count];
			for ( int i = 0; i < _resolutions.Length; i++ ) {
				_resolutions[i] = supportedResolutions[i].ToDisplayString();
			}

			_vsyncModes = new string[(int)VSyncMode.Count];
			for ( VSyncMode mode = 0; mode < VSyncMode.Count; mode++ ) {
				_vsyncModes[(int)mode] = mode.ToDisplayString();
			}

			var monitors = windowService.Monitors;
			_monitors = new string[monitors.Count];
			for ( int i = 0; i < monitors.Count; i++ ) {
				_monitors[i] = i.ToString();
			}
		}

		public void Save()
		{
			_service.Save();
		}

		public void Reset()
		{
			_service.ResetToDefault();
		}

		public void SetWindowResolution( WindowResolution value )
		{
			_service.Config.Resolution = value;
		}

		public void SetWindowMode( WindowMode value )
		{
			_service.Config.WindowMode = value;
		}

		public void SetVSyncMode( VSyncMode value )
		{
			_service.Config.VSyncMode = value;
		}

		public void SetMaximumFramerate( int value )
		{
			_service.Config.MaximumFrameRate = value;
		}

		public void SetMonitorIndex( int value )
		{
			_service.Config.MonitorIndex = value;
		}
	};
};
