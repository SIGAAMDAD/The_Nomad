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
using Nomad.Core.CVars;

namespace Nomad.Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	GraphicsCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class GraphicsCVars {
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
				new CVarCreateInfo<bool> {
					Name = Nomad.EngineUtils.Constants.CVars.BAKED_LIGHTS,
					DefaultValue = false,
					Description = "Forces the usage of Sprite2D lights with Additive CanvasModulate, if this is toggled on, shadows are disabled as PointLight2D nodes aren't used.",
					Flags = CVarFlags.Archive
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<float> {
					Name = Nomad.EngineUtils.Constants.CVars.SHADOW_FILTER_SMOOTH,
					DefaultValue = 0.0f,
					Description = "Sets in-game 2D shadow filtering smoothness.",
					Flags = CVarFlags.Archive,
					Validator = value => value >= 0.0f && value <= 1.0f
				}
			);
		}
	};
};
