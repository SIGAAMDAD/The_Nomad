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


using CVars;
using Godot;

namespace Settings.Config {
	/*
	===================================================================================
	
	Display
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static partial class Display {
		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<int> NativeDisplayWidth = new CVar<int>(
			name: "display.NativeDisplayWidth",
			defaultValue: DisplayServer.ScreenGetSize( DisplayServer.WindowGetCurrentScreen() ).X,
			description: "The actual width of the client's physical monitor.",
			flags: CVarFlags.ReadOnly
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<int> NativeDisplayHeight = new CVar<int>(
			name: "display.NativeDisplayHeight",
			defaultValue: DisplayServer.ScreenGetSize( DisplayServer.WindowGetCurrentScreen() ).Y,
			description: "The actual height of the client's physical monitor.",
			flags: CVarFlags.ReadOnly
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<int> Monitor = new CVar<int>(
			name: "display.Monitor",
			defaultValue: DisplayServer.WindowGetCurrentScreen(),
			description: "",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<WindowMode> WindowMode = new CVar<WindowMode>(
			name: "display.WindowMode",
			defaultValue: Settings.WindowMode.ExclusiveFullscreen,
			description: "The game's window mode.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<AspectRatio> AspectRatio = new CVar<AspectRatio>(
			name: "display.AspectRatio",
			defaultValue: Settings.AspectRatio.Aspect_Automatic,
			description: "The display aspect ratio.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<WindowResolution> Resolution = new CVar<WindowResolution>(
			name: "display.Resolution",
			defaultValue: Settings.WindowResolution.Res_640x480,
			description: "Size of the game's display window",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<VSyncMode> VSyncMode = new CVar<VSyncMode>(
			name: "display.VSyncMode",
			defaultValue: Settings.VSyncMode.Off,
			description: "Sets the engine's vertical sync policy",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<int> MaxFps = new CVar<int>(
			name: "display.MaxFps",
			defaultValue: 60,
			description: "Sets the maximum amount of gameplay loops per second, set to 0 for unlimited.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<AntiAliasing> AntiAliasing = new CVar<AntiAliasing>(
			name: "display.AntiAliasing",
			defaultValue: Settings.AntiAliasing.None,
			description: "Sets the renderer's method for reduces aliasing (jaggies) for the final displayed image.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<int> DRSTargetFrames = new CVar<int>(
			name: "display.DRSTargetFrames",
			defaultValue: 60,
			description: "",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<PerformanceOverlayPreset> PerformanceOverlay = new CVar<PerformanceOverlayPreset>(
			name: "display.PerformanceOverlay",
			defaultValue: PerformanceOverlayPreset.Hidden,
			description: "Enables an overlay to be drawn that displays .",
			flags: CVarFlags.Archive,
			value => value >= PerformanceOverlayPreset.Hidden && value < PerformanceOverlayPreset.Count
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> SeparateRenderingThread = new CVar<bool>(
			name: "r.SeparateRenderingThread", // put into display settings because putting it into graphics would be more confusing
			defaultValue: false,
			description: "Allows Godot to utilize a separate thread for its rendering pipeline. This is an experimental feature for Godot, and may result in slower performance and in some cases crash the application.",
			flags: CVarFlags.Archive
		);
	};
};