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
	
	Audio
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static partial class Audio {
		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> EffectsOn = new CVar<bool>(
			name: "audio.EffectsOn",
			defaultValue: true,
			description: "Enables sound effects.",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<float> EffectsVolume = new CVar<float>(
			name: "audio.EffectsVolume",
			defaultValue: 50.0f,
			description: "Sets sound effects volume.",
			flags: CVarFlags.Archive,
			value => value >= 0.0f && value <= 100.0f
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<bool> MusicOn = new CVar<bool>(
			name: "audio.MusicOn",
			defaultValue: true,
			description: "Enables music",
			flags: CVarFlags.Archive
		);

		/// <summary>
		/// 
		/// </summary>
		public readonly static CVar<float> MusicVolume = new CVar<float>(
			name: "audio.MusicVolume",
			defaultValue: 50.0f,
			description: "Sets music volume.",
			flags: CVarFlags.Archive,
			value => value >= 0.0f && value <= 100.0f
		);
	};
};