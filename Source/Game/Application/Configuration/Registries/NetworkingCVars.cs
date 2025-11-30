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
using System;

namespace Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	NetworkingCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class NetworkingCVars {
		/*
		===============
		Register
		===============
		*/
		public static void Register( ICVarSystemService cvarSystem ) {
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "networking.CODLobbies",
					defaultValue: false,
					description: "Enables unfiltered proximity chat.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "networking.ChatFilter",
					defaultValue: true,
					description: "Filters outgoing and incoming chat messages.",
					flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					name: "networking.EnableFakeLag",
					defaultValue: false,
					description: "Enables a simulation of lag, strictly for development purposes.",
					flags: CVarFlags.Hidden
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<int>(
					name: "networking.FakeLagMS",
					defaultValue: 0,
					description: "The amount of lag to emulate, requires networking.EnableFakeLag. Strictly for development purposes",
					flags: CVarFlags.Hidden
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					name: "networking.FakePacketLossMS",
					defaultValue: 0.0f,
					description: "Enables simulation of packet loss, strictly for development purposes.",
					flags: CVarFlags.Hidden
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<string>(
					name: "networking.LobbyMap",
					defaultValue: String.Empty,
					description: "The current lobby's map.",
					flags: CVarFlags.Hidden
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<string>(
					name: "networking.LobbyGameMode",
					defaultValue: String.Empty,
					description: "The current lobby's game mode.",
					flags: CVarFlags.Hidden
				)
			);
		}
	};
};