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

using Nomad.Core.CVars;

namespace Nomad.Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	GameplayCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class GameplayCVars {
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
				new CVarCreateInfo<float> {
					Name = "game.ScreenShakeIntensity",
					DefaultValue = 1.0f,
					Description = "Scales the intensity of how much the game will jitter the camera. Set to lower values for less jolting.",
					Flags = CVarFlags.Archive
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<int> {
					Name = "game.EnemyTacticalIntelligence",
					DefaultValue = 0,
					Description = "Controls how much planning can be executed for an enemy GOAP agent. Directly impacts performance.",
					Flags = CVarFlags.Archive
				}
			);
			cvarSystem.Register(
				new CVarCreateInfo<float> {
					Name = "game.PlayerDamageScale",
					DefaultValue = 1.0f,
					Description = "Scales how much damage the player receives.",
					Flags = CVarFlags.Archive
				}
			);
		}
	};
};
