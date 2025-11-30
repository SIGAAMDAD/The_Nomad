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

using Game.Application.Configuration.Enums;
using NomadCore.Abstractions.Services;
using NomadCore.Enums.ConsoleSystem;
using NomadCore.Utilities;
using System;

namespace Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	DisplayCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static class DisplayCVars {
		/*
		===============
		Register
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public static void Register( ICVarSystemService cvarSystem ) {
			cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "display.Monitor",
					defaultValue: 0,
					description: String.Empty,
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<WindowMode>(
					name: "display.WindowMode",
					defaultValue: WindowMode.ExclusiveFullscreen,
					description: "The game's window mode.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<WindowResolution>(
					name: "display.Resolution",
					defaultValue: WindowResolution.Res_640x480,
					description: "Size of the game's display window",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<AspectRatio>(
					name: "display.AspectRatio",
					defaultValue: AspectRatio.Aspect_Automatic,
					description: "The display aspect ratio.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<VSyncMode>(
					name: "display.VSyncMode",
					defaultValue: VSyncMode.Off,
					description: "Sets the engine's vertical sync policy",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<MaxFps>(
					name: "display.MaxFps",
					defaultValue: MaxFps.MaxFps60,
					description: "Sets the maximum amount of gameplay loops per second, set to 0 for unlimited.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<AntiAliasing>(
					name: "display.AntiAliasing",
					defaultValue: AntiAliasing.None,
					description: "Sets the renderer's method for reduces aliasing (jaggies) for the final displayed image.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "display.DRSTargetFrames",
					defaultValue: 60,
					description: String.Empty,
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<PerformanceOverlayPreset>(
					name: "display.PerformanceOverlay",
					defaultValue: PerformanceOverlayPreset.Hidden,
					description: "Enables an overlay to be drawn that displays .",
					flags: CVarFlags.Archive,
					validator: value => value >= PerformanceOverlayPreset.Hidden && value < PerformanceOverlayPreset.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.SeparateRenderingThread", // put into display settings because putting it into graphics would be more confusing
					defaultValue: false,
					description: "Allows Godot to utilize a separate thread for its rendering pipeline. This is an experimental feature for Godot, and may result in slower performance and in some cases crash the application.",
					flags: CVarFlags.Archive
				)
			);
		}
	};
};