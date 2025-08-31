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

using System.Collections.Generic;

namespace Menus.Settings {
	public readonly struct ShadowQuality {
		public readonly int ShadowTextureSize;
		public readonly bool TighterShadowCasterCulling;

		public static readonly IReadOnlyDictionary<global::ShadowQuality, ShadowQuality>? DataTable = new Dictionary<global::ShadowQuality, ShadowQuality> {
			{ global::ShadowQuality.Off, new ShadowQuality( 0, true ) },
			{ global::ShadowQuality.Low, new ShadowQuality( 2048, true ) },
			{ global::ShadowQuality.Medium, new ShadowQuality( 4096, false ) },
			{ global::ShadowQuality.High, new ShadowQuality( 8192, false ) },
			{ global::ShadowQuality.Ultra, new ShadowQuality( 12288, false ) },
		};

		/*
		===============
		ShadowQuality
		===============
		*/
		/// <summary>
		/// Initializes a Settings.ShadowQuality object
		/// </summary>
		public ShadowQuality( int shadowTextureSize, bool tighterShadowCasterCulling ) {
			ShadowTextureSize = shadowTextureSize;
			TighterShadowCasterCulling = tighterShadowCasterCulling;
		}
	};
};