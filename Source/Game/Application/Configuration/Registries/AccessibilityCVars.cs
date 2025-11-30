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

namespace Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	AccessibilityCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class AccessibilityCVars {
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
				new CVarCreateInfo<float>(
					name: "input.HapticStrength",
					defaultValue: 50.0f,
					description: "Sets the intensity of haptic feedback effects.",
					flags: CVarFlags.Archive,
					value => value >= 0.0f && value <= 100.0f
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "input.HapticEnabled",
					defaultValue: true,
					description: "Enables haptic feedback effects.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ColorblindMode>(
					name: "accessibility.ColorblindMode",
					defaultValue: ColorblindMode.None,
					description: "Sets the colorblind mode for the game, enabling makes different elements be set to varying color values.",
					flags: CVarFlags.Archive,
					validator: value => value >= ColorblindMode.None && value < ColorblindMode.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "accessibility.DyslexiaMode",
					defaultValue: false,
					description: "Switches all fonts in the game to the OpenDyslexia font.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "accessibility.UIScale",
					defaultValue: 1.0f,
					description: "Sets the scaling of in-game User Interface elements.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<AutoAimMode>(
					name: "accessibility.AutoAimMode",
					defaultValue: AutoAimMode.Off,
					description: "Sets aim assist algorithm that the game will utilize.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "accessibility.TextToSpeech",
					defaultValue: false,
					description: "Enables narration for in-game User Interface elements.",
					flags: CVarFlags.Archive
				)
			);
		}
	};
};