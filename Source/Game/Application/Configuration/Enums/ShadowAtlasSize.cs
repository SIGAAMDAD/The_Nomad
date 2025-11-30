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

namespace Game.Application.Configuration.Enums {
	public enum ShadowAtlasSize : byte {
		Size1024,
		Size2048,
		Size4096,
		Size8192,

		Count,

		Default = Size2048
	};

	public static class ShadowAtlasSizeExtensions {
		private static readonly Dictionary<ShadowAtlasSize, string> _atlasSizeStrings = new() {
			[ ShadowAtlasSize.Size1024 ] = "1024",
			[ ShadowAtlasSize.Size2048 ] = "2048",
			[ ShadowAtlasSize.Size4096 ] = "4096",
			[ ShadowAtlasSize.Size8192 ] = "8912"
		};

		/*
		===============
		ToDisplayString
		===============
		*/
		public static string ToDisplayString( this ShadowAtlasSize atlasSize ) {
			return _atlasSizeStrings[ atlasSize ];
		}
		
		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( string atlasSizeString, out ShadowAtlasSize atlasSize ) {
			foreach ( var match in _atlasSizeStrings ) {
				if ( match.Value.Equals( atlasSizeString, StringComparison.OrdinalIgnoreCase ) ) {
					atlasSize = match.Key;
					return true;
				}
			}
			atlasSize = ShadowAtlasSize.Default;
			return false;
		}
	};
};