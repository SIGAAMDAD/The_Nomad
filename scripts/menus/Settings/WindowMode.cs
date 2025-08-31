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
	/*
	===================================================================================
	
	WindowMode
	
	===================================================================================
	*/

	public readonly struct WindowMode {
		public readonly DisplayServer.WindowMode Mode;
		public readonly bool Borderless;

		public static readonly IReadOnlyDictionary<global::WindowMode, WindowMode>? DataTable = new Dictionary<global::WindowMode, WindowMode> {
			{ global::WindowMode.Windowed, WindowMode_Windowed },
			{ global::WindowMode.BorderlessWindowed, WindowMode_BorderlessWindowed },
			{ global::WindowMode.Fullscreen, WindowMode_Fullscreen },
			{ global::WindowMode.BorderlessFullscreen, WindowMode_BorderlessFullscreen },
			{ global::WindowMode.ExclusiveFullscreen, WindowMode_ExclusiveFullscreen }
		};

		private static readonly WindowMode WindowMode_Windowed = new WindowMode( DisplayServer.WindowMode.Windowed, false );
		private static readonly WindowMode WindowMode_BorderlessWindowed = new WindowMode( DisplayServer.WindowMode.Windowed, true );
		private static readonly WindowMode WindowMode_Fullscreen = new WindowMode( DisplayServer.WindowMode.Fullscreen, false );
		private static readonly WindowMode WindowMode_BorderlessFullscreen = new WindowMode( DisplayServer.WindowMode.Fullscreen, true );
		private static readonly WindowMode WindowMode_ExclusiveFullscreen = new WindowMode( DisplayServer.WindowMode.ExclusiveFullscreen, true );

		/*
		===============
		WindowMode
		===============
		*/
		/// <summary>
		/// Initializes a Settings.WindowMode object
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="borderless"></param>
		public WindowMode( DisplayServer.WindowMode mode, bool borderless ) {
			Mode = mode;
			Borderless = borderless;
		}
	};
};