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

using Game.Application.Common.Interfaces;
using Game.Application.Common.Models;
using NomadCore.Abstractions.Services;

namespace Game.Application.Configuration.Services {
	/*
	===================================================================================
	
	AudioSettingsService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class AudioSettingsService : IAudioSettingsService {
		private class AudioConfiguration( AudioSettingsService service ) : IAudioConfiguration {
			private readonly AudioSettingsService _service = service;

			public bool EffectsOn {
				get => _service._config.EffectsOn;
				set => _service._config = _service._config with { EffectsOn = value };
			}
			public bool MusicOn {
				get => _service._config.MusicOn;
				set => _service._config = _service._config with { MusicOn = value };
			}
			public float EffectsVolume {
				get => _service._config.EffectsVolume;
				set => _service._config = _service._config with { EffectsVolume = value };
			}
			public float MusicVolume {
				get => _service._config.MusicVolume;
				set => _service._config = _service._config with { MusicVolume = value };
			}
			public int OutputAudioDevice {
				get => _service._config.OutputDeviceIndex;
				set => _service._config = _service._config with { OutputDeviceIndex = value };
			}
			public string AudioDriver {
				get => _service._config.AudioDriver;
			}
		};

		public IAudioConfiguration Configuration => _configuration;
		private readonly AudioConfiguration _configuration;

		private AudioConfig _config;

		private readonly ICVarSystemService _cvarSystem;

		/*
		===============
		AudioSettingsService
		===============
		*/
		public AudioSettingsService( ICVarSystemService cvarSystem ) {
			_cvarSystem = cvarSystem;
			_configuration = new AudioConfiguration( this );

			_config = new AudioConfig() {
				EffectsVolume = _cvarSystem.GetCVar<float>( "audio.EffectsVolume" ).Value,
				MusicVolume = _cvarSystem.GetCVar<float>( "audio.MusicVolume" ).Value,
				EffectsOn = _cvarSystem.GetCVar<bool>( "audio.EffectsOn" ).Value,
				MusicOn = _cvarSystem.GetCVar<bool>( "audio.MusicOn").Value,
				AudioDriver = _cvarSystem.GetCVar<string>( "audio.AudioDriver" ).Value,
				OutputDeviceIndex = _cvarSystem.GetCVar<int>( "audio.OutputDeviceIndex" ).Value
			};
		}

		/*
		===============
		GetCurrentConfig
		===============
		*/
		public AudioConfig GetCurrentConfig() {
			return _config;
		}
	};
};