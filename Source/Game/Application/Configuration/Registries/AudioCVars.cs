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

using Nomad.CVars;
using Nomad.Core;
using System;

namespace Game.Application.Configuration.Registries {
	/*
	===================================================================================
	
	AudioCVars
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public static class AudioCVars {
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
					Name: Constants.CVars.Audio.MASTER_VOLUME,
					DefaultValue: 80.0f,
					Description: "The maximum volume output of the game.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= 0.0f && value <= 100.0f
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.EFFECTS_VOLUME,
					DefaultValue: 50.0f,
					Description: "Sets sound effects volume.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= 0.0f && value <= 100.0f
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Audio.EFFECTS_ON,
					DefaultValue: true,
					Description: "Enables sound effects.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.MUSIC_VOLUME,
					DefaultValue: 50.0f,
					Description: "Sets music volume.",
					Flags: CVarFlags.Archive,
					Validator: value => value >= 0.0f && value <= 100.0f
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<bool>(
					Name: Constants.CVars.Audio.MUSIC_ON,
					DefaultValue: true,
					Description: "Enables music.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<int>(
					Name: Constants.CVars.Audio.OUTPUT_DEVICE_INDEX,
					DefaultValue: 0,
					Description: "The device index of the output device to use for audio.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<string>(
					Name: Constants.CVars.Audio.AUDIO_DRIVER,
					DefaultValue: String.Empty,
					Description: "The active audio driver in use by the Audio system.",
					Flags: CVarFlags.Archive
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<int>(
					Name: Constants.CVars.Audio.MAX_ACTIVE_CHANNELS,
					DefaultValue: 256,
					Description: String.Empty,
					Flags: CVarFlags.Archive,
					Validator: value => value >= Constants.Audio.MIN_AUDIO_CHANNELS && value <= Constants.Audio.MAX_AUDIO_CHANNELS
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<int>(
					Name: Constants.CVars.Audio.MAX_CHANNELS,
					DefaultValue: 512,
					Description: String.Empty,
					Flags: CVarFlags.Init | CVarFlags.ReadOnly,
					Validator: value => value >= Constants.Audio.MIN_AUDIO_CHANNELS && value <= Constants.Audio.MAX_AUDIO_CHANNELS
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.DISTANCE_FALLOFF_START,
					DefaultValue: 50.0f,
					Description: String.Empty,
					Flags: CVarFlags.Init | CVarFlags.ReadOnly
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.DISTANCE_FALLOFF_END,
					DefaultValue: 100.0f,
					Description: String.Empty,
					Flags: CVarFlags.Init | CVarFlags.ReadOnly
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.MIN_TIME_BETWEEN_CHANNEL_STEALS,
					DefaultValue: 0.1f,
					Description: String.Empty,
					Flags: CVarFlags.Init | CVarFlags.ReadOnly
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.FREQUENCY_PENALTY,
					DefaultValue: 0.4f,
					Description: String.Empty,
					Flags: CVarFlags.Init | CVarFlags.ReadOnly
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.VOLUME_WEIGHT,
					DefaultValue: 0.2f,
					Description: String.Empty,
					Flags: CVarFlags.Init | CVarFlags.ReadOnly
				)
			);
			cvarSystem.Register(
				new CVarCreateInfo<float>(
					Name: Constants.CVars.Audio.DISTANCE_WEIGHT,
					DefaultValue: 0.3f,
					Description: String.Empty,
					Flags: CVarFlags.Init | CVarFlags.ReadOnly
				)
			);
		}
	};
};