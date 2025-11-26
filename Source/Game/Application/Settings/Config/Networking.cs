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

using CVars;

namespace Settings.Config {
	/*
	===================================================================================
	
	Networking
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static class Networking {
		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<bool> CODLobbies = new CVar<bool>(
			name: "networking.CODLobbies",
			defaultValue: false,
			description: "Enables unfiltered proximity chat.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<bool> ChatFilter = new CVar<bool>(
			name: "networking.ChatFilter",
			defaultValue: true,
			description: "Filters outgoing and incoming chat messages.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<string> LobbyMap = new CVar<string>(
			name: "networking.LobbyMap",
			defaultValue: "",
			description: "",
			flags: CVarFlags.Hidden
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<string> LobbyGameMode = new CVar<string>(
			name: "networking.LobbyGameMode",
			defaultValue: "",
			description: "",
			flags: CVarFlags.Hidden
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<bool> EnableFakeLag = new CVar<bool>(
			name: "networking.EnableFakeLag",
			defaultValue: false,
			description: "Enables a simulation of lag, strictly for development purposes.",
			flags: CVarFlags.None
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<int> FakeLagMS = new CVar<int>(
			name: "networking.FakeLagMS",
			defaultValue: 0,
			description: "",
			flags: CVarFlags.None
		);

		/// <summary>
		/// 
		/// </summary>
		public static readonly CVar<float> FakePacketLossMS = new CVar<float>(
			name: "networking.FakePacketLossMS",
			defaultValue: 0.0f,
			description: "",
			flags: CVarFlags.None
		);
	};
};