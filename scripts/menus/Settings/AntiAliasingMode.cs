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
	public readonly struct AntiAliasingMode {
		public readonly RenderingServer.ViewportMsaa ViewportMsaa;
		public readonly RenderingServer.ViewportScreenSpaceAA ScreenSpaceAA;
		public readonly bool UseTAA;

		public readonly static IReadOnlyDictionary<AntiAliasing, AntiAliasingMode>? DataTable = new Dictionary<AntiAliasing, AntiAliasingMode> {
			{ AntiAliasing.None, new AntiAliasingMode( RenderingServer.ViewportMsaa.Disabled, RenderingServer.ViewportScreenSpaceAA.Disabled, false ) },
			{ AntiAliasing.FXAA, new AntiAliasingMode( RenderingServer.ViewportMsaa.Disabled, RenderingServer.ViewportScreenSpaceAA.Fxaa, false ) },
			{ AntiAliasing.MSAA_2x, new AntiAliasingMode( RenderingServer.ViewportMsaa.Msaa2X, RenderingServer.ViewportScreenSpaceAA.Disabled, false ) },
			{ AntiAliasing.MSAA_4x, new AntiAliasingMode( RenderingServer.ViewportMsaa.Msaa4X, RenderingServer.ViewportScreenSpaceAA.Disabled, false ) },
			{ AntiAliasing.MSAA_8x, new AntiAliasingMode( RenderingServer.ViewportMsaa.Msaa8X, RenderingServer.ViewportScreenSpaceAA.Disabled, false ) },
			{ AntiAliasing.FXAA_and_TAA, new AntiAliasingMode( RenderingServer.ViewportMsaa.Disabled, RenderingServer.ViewportScreenSpaceAA.Fxaa, true ) },
			{ AntiAliasing.TAA, new AntiAliasingMode( RenderingServer.ViewportMsaa.Disabled, RenderingServer.ViewportScreenSpaceAA.Disabled, true ) },
		};

		/*
		===============
		AntiAliasingMode
		===============
		*/
		/// <summary>
		/// Initializes a Settings.AntiAliasingMode object
		/// </summary>
		/// <param name="viewportMsaa"></param>
		/// <param name="screenSpaceAA"></param>
		/// <param name="useTAA"></param>
		public AntiAliasingMode( RenderingServer.ViewportMsaa viewportMsaa, RenderingServer.ViewportScreenSpaceAA screenSpaceAA, bool useTAA ) {
			ViewportMsaa = viewportMsaa;
			ScreenSpaceAA = screenSpaceAA;
			UseTAA = useTAA;
		}
	};
};