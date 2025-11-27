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

using NomadCore.Abstractions.Services;
using NomadCore.Enums.ConsoleSystem;
using NomadCore.Interfaces;
using NomadCore.Interfaces.ConsoleSystem;
using NomadCore.Systems.ConsoleSystem.CVars.Common;
using NomadCore.Utilities;
using System;
using System.Collections.Generic;

namespace Game.Domain.DomainServices.Settings {
	/*
	===================================================================================
	
	Gameplay
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class Gameplay : ConfigHandler, IGameService {
		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> ScreenShakeIntensity;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> EnemyTacticalIntelligence;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> PlayerDamageScale;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> AutoInform;

		public override Dictionary<ICVar, object>? Cvars => _cvars.Value;
		private readonly Lazy<Dictionary<ICVar, object>?> _cvars;

		/*
		===============
		Gameplay
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public Gameplay( ICVarSystemService cvarSystem ) {
			ScreenShakeIntensity = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "game.ScreenShakeIntensity",
					defaultValue: 1.0f,
					description: "Scales the intensity of how much the game will jitter the camera. Set to lower values for less jolting.",
					flags: CVarFlags.Archive
				)
			);

			EnemyTacticalIntelligence = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "game.EnemyTacticalIntelligence",
					defaultValue: 0,
					description: "Controls how much planning can be executed for an enemy GOAP agent. Directly impacts performance.",
					flags: CVarFlags.Archive
				)
			);

			PlayerDamageScale = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "game.PlayerDamageScale",
					defaultValue: 1.0f,
					description: "Scales how much damage the player receives.",
					flags: CVarFlags.Archive
				)
			);

			AutoInform = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "game.AutoInform",
					defaultValue: false,
					description: "Automatically buys and places the Galakan Gossip newspaper in the player's inventory.",
					flags: CVarFlags.Archive
				)
			);

			_cvars = new Lazy<Dictionary<ICVar, object>?>(
				() => new Dictionary<ICVar, object> {
					{ EnemyTacticalIntelligence, 0 },
					{ PlayerDamageScale, 1.0f },
					{ ScreenShakeIntensity, 1.0f },
				}
			);
		}
	};
};