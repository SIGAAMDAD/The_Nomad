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

using Game.Domain.Settings.Enums;
using NomadCore.Abstractions.Services;
using NomadCore.Enums.ConsoleSystem;
using NomadCore.Interfaces;
using NomadCore.Interfaces.ConsoleSystem;
using NomadCore.Systems.ConsoleSystem.CVars.Common;
using NomadCore.Utilities;
using System.Collections.Generic;

namespace Game.Domain.Settings.ConfigServices {
	/*
	===================================================================================
	
	AccessibilityConfigService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class AccessibilityConfigService : ConfigHandler, IGameService {
		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> HapticStrength;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> HapticEnabled;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<ColorblindMode> ColorblindMode;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> DyslexiaMode;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> UIScale;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<AutoAimMode> AutoAimMode;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> TextToSpeech;

		public override Dictionary<ICVar, object>? Cvars => _cvars;
		private readonly Dictionary<ICVar, object>? _cvars;

		/*
		===============
		AccessibilityConfigService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public AccessibilityConfigService( ICVarSystemService cvarSystem ) {
			HapticStrength = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "input.HapticStrength",
					defaultValue: 50.0f,
					description: "Sets the intensity of haptic feedback effects.",
					flags: CVarFlags.Archive,
					value => value >= 0.0f && value <= 100.0f
				)
			);
			HapticEnabled = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "input.HapticEnabled",
					defaultValue: true,
					description: "Enables haptic feedback effects.",
					flags: CVarFlags.Archive
				)
			);
			ColorblindMode = (CVar<ColorblindMode>)cvarSystem.Register(
				new CVarCreateInfo<ColorblindMode>(
					name: "accessibility.ColorblindMode",
					defaultValue: Enums.ColorblindMode.None,
					description: "Sets the colorblind mode for the game, enabling makes different elements be set to varying color values.",
					flags: CVarFlags.Archive,
					value => value >= Enums.ColorblindMode.None && value < Enums.ColorblindMode.Count
				)
			);
			DyslexiaMode = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "accessibility.DyslexiaMode",
					defaultValue: false,
					description: "Switches all fonts in the game to the OpenDyslexia font.",
					flags: CVarFlags.Archive
				)
			);
			UIScale = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "accessibility.UIScale",
					defaultValue: 1.0f,
					description: "Sets the scaling of in-game User Interface elements.",
					flags: CVarFlags.Archive
				)
			);
			AutoAimMode = (CVar<AutoAimMode>)cvarSystem.Register(
				new CVarCreateInfo<AutoAimMode>(
					name: "accessibility.AutoAimMode",
					defaultValue: Enums.AutoAimMode.Off,
					description: "Sets aim assist algorithm that the game will utilize.",
					flags: CVarFlags.Archive
				)
			);
			TextToSpeech = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "accessibility.TextToSpeech",
					defaultValue: false,
					description: "Enables narration for in-game User Interface elements.",
					flags: CVarFlags.Archive
				)
			);

			_cvars = new Dictionary<ICVar, object>() {
				{ DyslexiaMode, false },
				{ ColorblindMode, Enums.ColorblindMode.None },
				{ HapticStrength, 50.0f },
				{ HapticEnabled, true },
				{ AutoAimMode, Enums.AutoAimMode.Off },
				{ UIScale, 0.0f },
				{ TextToSpeech, false }
			};
		}
	};
};