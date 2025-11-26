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
using NomadCore.Abstractions.Services;
using NomadCore.Enums.ConsoleSystem;
using NomadCore.Infrastructure;
using NomadCore.Interfaces;
using NomadCore.Utilities;
using NomadCore.Systems.ConsoleSystem.CVars.Common;
using NomadCore.Interfaces.ConsoleSystem;
using Game.Domain.Settings.Enums;
using System;
using System.Collections.Generic;

namespace Game.Domain.Settings {
	/*
	===================================================================================
	
	DisplayConfigService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class DisplayConfigService : ConfigHandler, IGameService {
		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> NativeDisplayWidth;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> NativeDisplayHeight;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> Monitor;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<WindowMode> WindowMode;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<AspectRatio> AspectRatio;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<WindowResolution> Resolution;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<VSyncMode> VSyncMode;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> MaxFps;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<AntiAliasing> AntiAliasing;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> DRSTargetFrames;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<PerformanceOverlayPreset> PerformanceOverlay;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> SeparateRenderingThread;

		public override Dictionary<ICVar, object>? Cvars => _cvars;
		private readonly Dictionary<ICVar, object>? _cvars;

		/*
		===============
		DisplayConfigService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public DisplayConfigService( ICVarSystemService cvarSystem ) {
			NativeDisplayWidth = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "display.NativeDisplayWidth",
					defaultValue: DisplayServer.ScreenGetSize( DisplayServer.WindowGetCurrentScreen() ).X,
					description: "The actual width of the client's physical monitor.",
					flags: CVarFlags.ReadOnly
				)
			);
			NativeDisplayHeight = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "display.NativeDisplayHeight",
					defaultValue: DisplayServer.ScreenGetSize( DisplayServer.WindowGetCurrentScreen() ).Y,
					description: "The actual height of the client's physical monitor.",
					flags: CVarFlags.ReadOnly
				)
			);
			Monitor = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "display.Monitor",
					defaultValue: DisplayServer.WindowGetCurrentScreen(),
					description: "",
					flags: CVarFlags.Archive
				)
			);
			WindowMode = (CVar<WindowMode>)cvarSystem.Register(
				new CVarCreateInfo<WindowMode>(
					name: "display.WindowMode",
					defaultValue: Enums.WindowMode.ExclusiveFullscreen,
					description: "The game's window mode.",
					flags: CVarFlags.Archive
				)
			);
			AspectRatio = (CVar<AspectRatio>)cvarSystem.Register(
				new CVarCreateInfo<AspectRatio>(
					name: "display.AspectRatio",
					defaultValue: Enums.AspectRatio.Aspect_Automatic,
					description: "The display aspect ratio.",
					flags: CVarFlags.Archive
				)
			);
			Resolution = (CVar<WindowResolution>)cvarSystem.Register(
				new CVarCreateInfo<WindowResolution>(
					name: "display.Resolution",
					defaultValue: WindowResolution.Res_640x480,
					description: "Size of the game's display window",
					flags: CVarFlags.Archive
				)
			);
			VSyncMode = (CVar<VSyncMode>)cvarSystem.Register(
				new CVarCreateInfo<VSyncMode>(
					name: "display.VSyncMode",
					defaultValue: Enums.VSyncMode.Off,
					description: "Sets the engine's vertical sync policy",
					flags: CVarFlags.Archive
				)
			);
			MaxFps = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "display.MaxFps",
					defaultValue: 60,
					description: "Sets the maximum amount of gameplay loops per second, set to 0 for unlimited.",
					flags: CVarFlags.Archive
				)
			);
			AntiAliasing = (CVar<AntiAliasing>)cvarSystem.Register(
				new CVarCreateInfo<AntiAliasing>(
					name: "display.AntiAliasing",
					defaultValue: Enums.AntiAliasing.None,
					description: "Sets the renderer's method for reduces aliasing (jaggies) for the final displayed image.",
					flags: CVarFlags.Archive
				)
			);
			DRSTargetFrames = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "display.DRSTargetFrames",
					defaultValue: 60,
					description: "",
					flags: CVarFlags.Archive
				)
			);
			PerformanceOverlay = (CVar<PerformanceOverlayPreset>)cvarSystem.Register(
				new CVarCreateInfo<PerformanceOverlayPreset>(
					name: "display.PerformanceOverlay",
					defaultValue: PerformanceOverlayPreset.Hidden,
					description: "Enables an overlay to be drawn that displays .",
					flags: CVarFlags.Archive,
					validator: value => value >= PerformanceOverlayPreset.Hidden && value < PerformanceOverlayPreset.Count
				)
			);
			SeparateRenderingThread = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "r.SeparateRenderingThread", // put into display settings because putting it into graphics would be more confusing
					defaultValue: false,
					description: "Allows Godot to utilize a separate thread for its rendering pipeline. This is an experimental feature for Godot, and may result in slower performance and in some cases crash the application.",
					flags: CVarFlags.Archive
				)
			);

			_cvars = new Dictionary<ICVar, object>() {
				{ WindowMode, Enums.WindowMode.ExclusiveFullscreen },
				{ Resolution, WindowResolution.Res_Native },
				{ AntiAliasing, Enums.AntiAliasing.FXAA },
				{ AspectRatio, Enums.AspectRatio.Aspect_Automatic },
				{ MaxFps, 60 },
				{ VSyncMode, Enums.VSyncMode.On },
				{ Monitor, 0 },
				{ SeparateRenderingThread, false },
				{ PerformanceOverlay, PerformanceOverlayPreset.Hidden }
			};
		}
	};
};