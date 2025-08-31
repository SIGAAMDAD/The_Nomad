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
using System.Collections.Generic;

namespace Menus.Settings {
	public readonly struct VSyncMode {
		public readonly int SwapChainImageCount;
		public readonly DisplayServer.VSyncMode Mode;
		
		public static readonly IReadOnlyDictionary<global::VSyncMode, VSyncMode>? DataTable = new Dictionary<global::VSyncMode, VSyncMode> {
			{ global::VSyncMode.On, VSyncMode_On },
			{ global::VSyncMode.Off, VSyncMode_Off },
			{ global::VSyncMode.Adaptive, VSyncMode_Adaptive },
			{ global::VSyncMode.TripleBuffered, VSyncMode_TripleBuffered }
		};

		private static readonly VSyncMode VSyncMode_On = new VSyncMode( 2, DisplayServer.VSyncMode.Enabled );
		private static readonly VSyncMode VSyncMode_Off = new VSyncMode( 2, DisplayServer.VSyncMode.Disabled );
		private static readonly VSyncMode VSyncMode_Adaptive = new VSyncMode( 2, DisplayServer.VSyncMode.Adaptive );
		private static readonly VSyncMode VSyncMode_TripleBuffered = new VSyncMode( 3, DisplayServer.VSyncMode.Enabled );

		/*
		===============
		VSyncMode
		===============
		*/
		/// <summary>
		/// Initializes a Settings.VSyncMode object
		/// </summary>
		/// <param name="swapChainImageCount"></param>
		/// <param name="mode"></param>
		public VSyncMode( int swapChainImageCount, DisplayServer.VSyncMode mode ) {
			SwapChainImageCount = swapChainImageCount;
			Mode = mode;
		}
	};
};