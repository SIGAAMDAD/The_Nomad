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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Game.Application.Configuration.Enums {
	public enum MaxFps : byte {
		MaxFps30,
		MaxFps48,
		MaxFps60,
		MaxFps90,
		MaxFps125,
		MaxFps172,
		MaxFps225,
		MaxFps333,

		Count,

		Default = MaxFps60
	};
	
	public static class MaxFpsExtensions {
		private static readonly Dictionary<MaxFps, string> _maxFpsStrings = new() {
			[ MaxFps.MaxFps30 ] = "30",
			[ MaxFps.MaxFps48 ] = "48",
			[ MaxFps.MaxFps60 ] = "60",
			[ MaxFps.MaxFps90 ] = "90",
			[ MaxFps.MaxFps125 ] = "125",
			[ MaxFps.MaxFps172 ] = "172",
			[ MaxFps.MaxFps225 ] = "225",
			[ MaxFps.MaxFps333 ] = "333"
		};

		/*
		===============
		ToDisplayString
		===============
		*/
		public static string ToDisplayString( this MaxFps maxFps ) {
			return _maxFpsStrings[ maxFps ];
		}
		
		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( string maxFpsString, out MaxFps maxFps ) {
			foreach ( var match in _maxFpsStrings ) {
				if ( match.Value.Equals( maxFpsString, StringComparison.OrdinalIgnoreCase ) ) {
					maxFps = match.Key;
					return true;
				}
			}
			maxFps = MaxFps.Default;
			return false;
		}
	};
};