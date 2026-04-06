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

using Nomad.Core.Engine.Rendering;
using Nomad.Core.Engine.Services;
using Nomad.Core.Engine.Windowing;
using Nomad.Core.ServiceRegistry.Globals;
using Nomad.CVars;
using Nomad.CVars.Global;
using Nomad.Game.Infrastructure.UI.Nodes.OptionList;
using Nomad.Game.Infrastructure.UI.Nodes.OptionSlider;
using Nomad.UI;
using System;

namespace Nomad.Game.Presentation.Screens.SettingsMenu {
	/*
	===================================================================================
	
	DisplayOptionsContainer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class DisplayOptionsContainer : EngineTabContainer {
		protected override void OnInit() {
			base.OnInit();

			InitBasicDisplayOptions();
		}

		private void InitBasicDisplayOptions() {
			var cvarSystem = CVarSystem.Instance;
			var windowService = ServiceLocator.GetService<IWindowService>();

			var monitors = new string[windowService.Monitors.Count];
			for ( int i = 0; i < monitors.Length; i++ ) {
				monitors[i] = i.ToString();
			}

			var monitorIndex = FindChild<OptionList>( "Basic/MonitorList" );
			monitorIndex.SetOptions( monitors );
			monitorIndex.Value = cvarSystem.GetCVarOrThrow<int>( Core.Constants.CVars.EngineUtils.Display.MONITOR ).Value;

			var windowModes = new string[] {
				"Windowed",
				"Borderless Windowed",
				"Fullscreen"
			};
			var windowMode = FindChild<OptionList>( "Basic/WindowModeList" );
			windowMode.SetOptions( windowModes );
			windowMode.Value = cvarSystem.GetCVarOrThrow<WindowMode>( Core.Constants.CVars.EngineUtils.Display.WINDOW_MODE ).Value switch {
				WindowMode.Windowed => 0,
				WindowMode.BorderlessWindowed => 1,
				WindowMode.ExclusiveFullscreen => 2,
				_ => throw new ArgumentOutOfRangeException( nameof( windowMode.Value ) )
			};

			var supportedResolutions = windowService.GetSupportedResolutions( monitorIndex.Value );
			var windowResolutions = new string[supportedResolutions.Count];
			for ( int i = 0; i < windowResolutions.Length; i++ ) {
				windowResolutions[i] = supportedResolutions[i].ToDisplayString();
			}
			var windowResolution = FindChild<OptionList>( "Basic/WindowResolutionList" );
			windowResolution.SetOptions( windowResolutions );
			windowResolution.Value = (int)cvarSystem.GetCVarOrThrow<WindowResolution>( Core.Constants.CVars.EngineUtils.Display.WINDOW_RESOLUTION ).Value;

			var vsyncModes = new string[(int)VSyncMode.Count];
			for ( int i = 0; i < vsyncModes.Length; i++ ) {
				var mode = (VSyncMode)((int)VSyncMode.Enabled + i);
				vsyncModes[i] = mode.ToDisplayString();
			}

			var vsyncList = FindChild<OptionList>( "Basic/VSyncList" );
			vsyncList.SetOptions( vsyncModes );
			vsyncList.Value = (int)cvarSystem.GetCVarOrThrow<VSyncMode>( Core.Constants.CVars.EngineUtils.Display.VSYNC_MODE ).Value;

			var maximumFramerate = FindChild<OptionSlider>( "Basic/MaxFpsSlider" );
			maximumFramerate.Min = 30.0f;
			maximumFramerate.Max = 600.0f;
			maximumFramerate.Value = cvarSystem.GetCVarOrThrow<int>( Core.Constants.CVars.EngineUtils.Display.MAX_FPS ).Value;

			var aspectRatios = new string[(int)AspectRatio.Count];
			for ( int i = 0; i < aspectRatios.Length; i++ ) {
				var ratio = (AspectRatio)((int)AspectRatio.Aspect_4_3 + i);
				aspectRatios[i] = ratio.ToDisplayString();
			}
			var aspectRatio = FindChild<OptionList>( "Basic/AspectRatioList" );
			aspectRatio.SetOptions( aspectRatios );
			aspectRatio.Value = (int)cvarSystem.GetCVarOrThrow<AspectRatio>( Core.Constants.CVars.EngineUtils.Display.ASPECT_RATIO ).Value;
		}
	};
};
