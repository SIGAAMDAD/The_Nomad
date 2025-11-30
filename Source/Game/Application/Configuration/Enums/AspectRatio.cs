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

using Game.Application.Common;
using System.Collections.Generic;

namespace Game.Application.Configuration.Enums {
	public enum AspectRatio : byte {
		Aspect_Automatic,
		Aspect_4_3,
		Aspect_16_10,
		Aspect_16_9,
		Aspect_21_9,

		Count,

		Default = Aspect_Automatic
	};

	public static class AspectRatioExtensions {
		private static readonly Dictionary<AspectRatio, string> _aspectRatioStrings = new() {
			[ AspectRatio.Aspect_Automatic ] = TranslationKeys.Display.AspectRatioAutomatic,
			[ AspectRatio.Aspect_4_3 ] = "4:3",
			[ AspectRatio.Aspect_16_10 ] = "16:10",
			[ AspectRatio.Aspect_16_9 ] = "16:9",
			[ AspectRatio.Aspect_21_9 ] = "21:9"
		};

		/*
		===============
		ToDisplayString
		===============
		*/
		public static string ToDisplayString( this AspectRatio aspectRatio ) {
			return _aspectRatioStrings[ aspectRatio ];
		}
		
		/*
		===============
		TryParse
		===============
		*/
		public static bool TryParse( string aspectRatioString, out AspectRatio aspectRatio ) {
			foreach ( var match in _aspectRatioStrings ) {
				if ( match.Value.Equals( aspectRatioString, System.StringComparison.OrdinalIgnoreCase ) ) {
					aspectRatio = match.Key;
					return true;
				}
			}
			aspectRatio = AspectRatio.Default;
			return false;
		}
	};
};