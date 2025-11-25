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
using Settings;
using Settings.Config;
using System.Collections.Generic;

namespace Menus.Settings.Updaters {
	/*
	===================================================================================
	
	DisplayUpdateer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public readonly struct DisplayUpdater {
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
		private static readonly float[] AspectRatios = [
			0.0f, // automatic
			(float)( 4.0f / 3.0f ),
			(float)( 16.0f / 10.0f ),
			(float)( 16.0f / 9.0f ),
			(float)( 21.0f / 9.0f )
		];

		/*
		===============
		DisplayUpdater
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public DisplayUpdater() {
			Display.WindowMode.ValueChanged.Subscribe( this, OnWindowModeChanged );
			Display.Resolution.ValueChanged.Subscribe( this, OnWindowResolutionChanged );
			Display.AntiAliasing.ValueChanged.Subscribe( this, OnAntiAliasingChanged );
			Display.AspectRatio.ValueChanged.Subscribe( this, OnAspectRatioChanged );
			Display.PerformanceOverlay.ValueChanged.Subscribe( this, OnPerformanceOverlayChanged );
			Display.SeparateRenderingThread.ValueChanged.Subscribe( this, OnSeparateRenderingThreadChanged );
			Display.VSyncMode.ValueChanged.Subscribe( this, OnVSyncModeChanged );
		}
	};
};