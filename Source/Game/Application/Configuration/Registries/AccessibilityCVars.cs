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
using Nomad.Core;
using Nomad.CVars;
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
					Name: Constants.CVars.Accessibility.HAPTIC_STRENGTH,
					DefaultValue: 50.0f,
					Description: "Sets the intensity of haptic feedback effects.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= 0.0f && value <= 100.0f
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Accessibility.HAPTIC_ENABLED,
					DefaultValue: true,
					Description: "Enables haptic feedback effects.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<ColorblindMode>(
					Name: Constants.CVars.Accessibility.COLORBLIND_MODE,
					DefaultValue: ColorblindMode.None,
					Description: "Sets the colorblind mode for the game, enabling makes different elements be set to varying color values.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= ColorblindMode.None && value < ColorblindMode.Count
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Accessibility.DYSLEXIA_MODE,
					DefaultValue: false,
					Description: "Switches all fonts in the game to the OpenDyslexia font.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Accessibility.UI_SCALE,
					DefaultValue: 1.0f,
					Description: "Sets the scaling of in-game User Interface elements.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<AutoAimMode>(
					Name: Constants.CVars.Accessibility.AUTO_AIM_MODE,
					DefaultValue: AutoAimMode.Off,
					Description: "Sets aim assist algorithm that the game will utilize.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Accessibility.TEXT_TO_SPEECH,
					DefaultValue: false,
					Description: "Enables narration for in-game User Interface elements.",
					Flags: CVarFlags.Archive
				)
			);
		}
	};
};