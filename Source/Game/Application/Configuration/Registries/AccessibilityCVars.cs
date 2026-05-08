/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using Nomad.Game.Application.Configuration.Enums;
using Nomad.Core;
using Nomad.Core.CVars;

namespace Nomad.Game.Application.Configuration.Registries
{
	/*
	===================================================================================
	
	AccessibilityCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class AccessibilityCVars
	{
		/*
		===============
		Register
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public static void Register( ICVarSystemService cvarSystem )
		{
			cvarSystem.Register(
				new CVarCreateInfo<float> {
					Name = Constants.CVars.EngineUtils.Accessibility.HAPTIC_STRENGTH,
					DefaultValue = 50.0f,
					Description = "Sets the intensity of haptic feedback effects.",
					Flags = CVarFlags.Archive,
					Validator = value => value >= 0.0f && value <= 100.0f
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool> {
					Name = Constants.CVars.EngineUtils.Accessibility.HAPTIC_ENABLED,
					DefaultValue = true,
					Description = "Enables haptic feedback effects.",
					Flags = CVarFlags.Archive
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<ColorblindMode> {
					Name = Constants.CVars.EngineUtils.Accessibility.COLORBLIND_MODE,
					DefaultValue = ColorblindMode.None,
					Description = "Sets the colorblind mode for the game, enabling makes different elements be set to varying color values.",
					Flags = CVarFlags.Archive,
					Validator = value => value >= ColorblindMode.None && value < ColorblindMode.Count
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool> {
					Name = Constants.CVars.EngineUtils.Accessibility.DYSLEXIA_MODE,
					DefaultValue = false,
					Description = "Switches all fonts in the game to the OpenDyslexia font.",
					Flags = CVarFlags.Archive
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<float> {
					Name = Constants.CVars.EngineUtils.Accessibility.UI_SCALE,
					DefaultValue = 1.0f,
					Description = "Sets the scaling of in-game User Interface elements.",
					Flags = CVarFlags.Archive
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<AutoAimMode> {
					Name = Constants.CVars.EngineUtils.Accessibility.AUTO_AIM_MODE,
					DefaultValue = AutoAimMode.Off,
					Description = "Sets aim assist algorithm that the game will utilize.",
					Flags = CVarFlags.Archive
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool> {
					Name = Constants.CVars.EngineUtils.Accessibility.TEXT_TO_SPEECH,
					DefaultValue = false,
					Description = "Enables narration for in-game User Interface elements.",
					Flags = CVarFlags.Archive
				}
			);
		}
	};
};
