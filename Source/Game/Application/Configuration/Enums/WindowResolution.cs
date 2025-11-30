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
using System.Collections.Generic;
using System.Numerics;

namespace Game.Application.Configuration.Enums {
	public enum WindowResolution : byte {
		Res_640x480,
		Res_800x600,
		Res_1024x768,
		Res_1280x720,
		Res_1280x768,
		Res_1280x800,
		Res_1280x1024,
		Res_1360x768,
		Res_1366x768,
		Res_1440x900,
		Res_1536x864,
		Res_1600x900,
		Res_1600x1200,
		Res_1680x1050,
		Res_1920x1080,
		Res_1920x1200,
		Res_2048x1152,
		Res_2048x1536,
		Res_2560x1080,
		Res_2560x1440,
		Res_2560x1600,
		Res_3440x1440,
		Res_3840x2160,

		Count,

		Default = Res_1920x1080
	};

	public static class WindowResolutionExtensions {
		private static readonly Dictionary<WindowResolution, string> _resolutionStrings = new() {
			[ WindowResolution.Res_640x480 ] = "640x480",
			[ WindowResolution.Res_800x600 ] = "800x600",
			[ WindowResolution.Res_1024x768 ] = "1024x768",
			[ WindowResolution.Res_1280x720 ] = "1280x720",
			[ WindowResolution.Res_1280x768 ] = "1280x768",
			[ WindowResolution.Res_1280x800 ] = "1280x800",
			[ WindowResolution.Res_1280x1024 ] = "1280x1024",
			[ WindowResolution.Res_1360x768 ] = "1360x768",
			[ WindowResolution.Res_1366x768 ] = "1366x768",
			[ WindowResolution.Res_1440x900 ] = "1440x900",
			[ WindowResolution.Res_1536x864 ] = "1536x864",
			[ WindowResolution.Res_1600x900 ] = "1600x900",
			[ WindowResolution.Res_1600x1200 ] = "1600x1200",
			[ WindowResolution.Res_1680x1050 ] = "1680x1050",
			[ WindowResolution.Res_1920x1080 ] = "1920x1080",
			[ WindowResolution.Res_1920x1200 ] = "1920x1200",
			[ WindowResolution.Res_2048x1152 ] = "2048x1152",
			[ WindowResolution.Res_2048x1536 ] = "2048x1536",
			[ WindowResolution.Res_2560x1080 ] = "2560x1080",
			[ WindowResolution.Res_2560x1440 ] = "2560x1440",
			[ WindowResolution.Res_2560x1600 ] = "2560x1600",
			[ WindowResolution.Res_3440x1440 ] = "3440x1440",
			[ WindowResolution.Res_3840x2160 ] = "3840x2160"
		};
		private static readonly Dictionary<WindowResolution, WindowSize> _resolutionSizes = new() {
			[ WindowResolution.Res_640x480 ] = new WindowSize( 640, 480 ),
			[ WindowResolution.Res_800x600 ] = new WindowSize( 800, 600 ),
			[ WindowResolution.Res_1024x768 ] = new WindowSize( 1024, 768 ),
			[ WindowResolution.Res_1280x720 ] = new WindowSize( 1280, 720 ),
			[ WindowResolution.Res_1280x768 ] = new WindowSize( 1280, 768 ),
			[ WindowResolution.Res_1280x800 ] = new WindowSize( 1280, 800 ),
			[ WindowResolution.Res_1280x1024 ] = new WindowSize( 1280, 1024 ),
			[ WindowResolution.Res_1360x768 ] = new WindowSize( 1360, 768 ),
			[ WindowResolution.Res_1366x768 ] = new WindowSize( 1366, 768 ),
			[ WindowResolution.Res_1440x900 ] = new WindowSize( 1440, 900 ),
			[ WindowResolution.Res_1536x864 ] = new WindowSize( 1536, 864 ),
			[ WindowResolution.Res_1600x900 ] = new WindowSize( 1600, 900 ),
			[ WindowResolution.Res_1600x1200 ] = new WindowSize( 1600, 1200 ),
			[ WindowResolution.Res_1680x1050 ] = new WindowSize( 1680, 1050 ),
			[ WindowResolution.Res_1920x1080 ] = new WindowSize( 1920, 1080 ),
			[ WindowResolution.Res_1920x1200 ] = new WindowSize( 1920, 1200 ),
			[ WindowResolution.Res_2048x1152 ] = new WindowSize( 2048, 1152 ),
			[ WindowResolution.Res_2048x1536 ] = new WindowSize( 2048, 1536 ),
			[ WindowResolution.Res_2560x1080 ] = new WindowSize( 2560, 1080 ),
			[ WindowResolution.Res_2560x1440 ] = new WindowSize( 2560, 1440 ),
			[ WindowResolution.Res_2560x1600 ] = new WindowSize( 2560, 1600 ),
			[ WindowResolution.Res_3440x1440 ] = new WindowSize( 3440, 1440 ),
			[ WindowResolution.Res_3840x2160 ] = new WindowSize( 3840, 2160 )
		};

		/*
		===============
		ToDisplayString
		===============
		*/
		public static string ToDisplayString( this WindowResolution resolution ) {
			return _resolutionStrings[ resolution ];
		}
		
		/*
		===============
		GetSize
		===============
		*/
		public static WindowSize GetSize( this WindowResolution resolution ) {
			return _resolutionSizes[ resolution ];
		}
		
		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( string resolutionString, out WindowResolution resolution ) {
			foreach ( var match in _resolutionStrings ) {
				if ( match.Value.Equals( resolutionString, System.StringComparison.OrdinalIgnoreCase ) ) {
					resolution = match.Key;
					return true;
				}
			}
			resolution = WindowResolution.Default;
			return false;
		}
	};
};