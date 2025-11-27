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
	
	Networking
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class Networking : ConfigHandler, IGameService {
		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> CODLobbies;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> ChatFilter;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<string> LobbyMap;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<string> LobbyGameMode;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<bool> EnableFakeLag;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<int> FakeLagMS;

		/// <summary>
		/// 
		/// </summary>
		public readonly CVar<float> FakePacketLossMS;

		public override Dictionary<ICVar, object>? Cvars => _cvars.Value;
		private readonly Lazy<Dictionary<ICVar, object>?> _cvars;

		/*
		===============
		Networking
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public Networking( ICVarSystemService cvarSystem ) {
			CODLobbies = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "networking.CODLobbies",
					defaultValue: false,
					description: "Enables unfiltered proximity chat.",
					flags: CVarFlags.Archive
				)
			);

			ChatFilter = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "networking.ChatFilter",
					defaultValue: true,
					description: "Filters outgoing and incoming chat messages.",
					flags: CVarFlags.Archive
				)
			);

			EnableFakeLag = (CVar<bool>)cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "networking.EnableFakeLag",
					defaultValue: false,
					description: "Enables a simulation of lag, strictly for development purposes.",
					flags: CVarFlags.Hidden
				)
			);

			FakeLagMS = (CVar<int>)cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "networking.FakeLagMS",
					defaultValue: 0,
					description: "The amount of lag to emulate, requires networking.EnableFakeLag. Strictly for development purposes",
					flags: CVarFlags.Hidden
				)
			);

			FakePacketLossMS = (CVar<float>)cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "networking.FakePacketLossMS",
					defaultValue: 0.0f,
					description: "Enables simulation of packet loss, strictly for development purposes.",
					flags: CVarFlags.Hidden
				)
			);

			LobbyMap = (CVar<string>)cvarSystem.Register(
				new CVarCreateInfo<string>(
					name: "networking.LobbyMap",
					defaultValue: String.Empty,
					description: "The current lobby's map.",
					flags: CVarFlags.Hidden
				)
			);

			LobbyGameMode = (CVar<string>)cvarSystem.Register(
				new CVarCreateInfo<string>(
					name: "networking.LobbyGameMode",
					defaultValue: String.Empty,
					description: "The current lobby's game mode.",
					flags: CVarFlags.Hidden
				)
			);

			_cvars = new Lazy<Dictionary<ICVar, object>?>(
				() => new Dictionary<ICVar, object>() {
					{ CODLobbies, false },
					{ ChatFilter, true }
				}
			);
		}
	};
};