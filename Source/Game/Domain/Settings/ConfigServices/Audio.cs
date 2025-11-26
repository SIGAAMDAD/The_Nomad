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
using NomadCore.Utilities;
using NomadCore.Interfaces;
using NomadCore.Interfaces.ConsoleSystem;
using NomadCore.Systems.ConsoleSystem.CVars.Common;
using System.Collections.Generic;

namespace Game.Domain.Settings.ConfigServices {
	/*
	===================================================================================
	
	Audio
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class Audio : ConfigHandler, IGameService {
		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> EffectsOn;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> EffectsVolume;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> MusicOn;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> MusicVolume;

		public override Dictionary<ICVar, object>? Cvars => _cvars;
		private readonly Dictionary<ICVar, object>? _cvars;

		/*
		===============
		Audio
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public Audio( ICVarSystemService cvarSystem ) {
			EffectsOn = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "audio.EffectsOn",
					defaultValue: true,
					description: "Enables sound effects.",
					flags: CVarFlags.Archive
				)
			);
			EffectsVolume = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "audio.EffectsVolume",
					defaultValue: 50.0f,
					description: "Sets sound effects volume.",
					flags: CVarFlags.Archive,
					value => value >= 0.0f && value <= 100.0f
				)
			);
			MusicOn = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "audio.MusicOn",
					defaultValue: true,
					description: "Enables music.",
					flags: CVarFlags.Archive
				)
			);
			MusicVolume = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "audio.MusicVolume",
					defaultValue: 50.0f,
					description: "Sets music volume.",
					flags: CVarFlags.Archive,
					value => value >= 0.0f && value <= 100.0f
				)
			);

			_cvars = new Dictionary<ICVar, object>() {
				{ EffectsOn, true },
				{ EffectsVolume, 50.0f },
				{ MusicOn, true },
				{ MusicVolume, 50.0f }
			};
		}
	};
};