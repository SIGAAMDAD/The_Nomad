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
	public readonly struct LightingQuality {
		public readonly bool ForceLambertOverBurley;

		public static readonly IReadOnlyDictionary<global::LightingQuality, LightingQuality>? DataTable = new Dictionary<global::LightingQuality, LightingQuality> {
			{ global::LightingQuality.VeryLow, new LightingQuality( true ) },
			{ global::LightingQuality.Low, new LightingQuality( true ) },
			{ global::LightingQuality.High, new LightingQuality( false ) }
		};

		/*
		===============
		LightingQuality
		===============
		*/
		/// <summary>
		/// Initializes a Settings.LightingQuality object
		/// </summary>
		/// <param name="forceLambertOverBurley"></param>
		public LightingQuality( bool forceLambertOverBurley ) {
			ForceLambertOverBurley = forceLambertOverBurley;
		}
	};
};