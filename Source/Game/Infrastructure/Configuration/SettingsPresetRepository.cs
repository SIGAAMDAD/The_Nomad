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

using Game.Application.Configuration.Enums;
using Game.Domain.Configuration;
using Game.Domain.Configuration.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Game.Infrastructure.Configuration {
	/*
	===================================================================================
	
	SettingsPresetRepository
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class SettingsPresetRepository : ISettingsPresetRepository {
		private readonly Dictionary<QualitySetting, SettingsPresetConfiguration> _presets;

		public SettingsPresetRepository() {
			_presets = new Dictionary<QualitySetting, SettingsPresetConfiguration>() {
				[ QualitySetting.Low ] = CreateLowQualityPreset(),
			};
		}

		/*
		===============
		GetPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="preset"></param>
		/// <returns></returns>
		public SettingsPresetConfiguration GetPreset( QualitySetting preset ) {
			return _presets[ preset ];
		}

		/*
		===============
		DetectPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentValues"></param>
		/// <returns></returns>
		public QualitySetting DetectPreset( IReadOnlyDictionary<string, string> currentValues ) {
			foreach ( var preset in _presets.Values ) {
				if ( MatchesPreset( currentValues, preset ) ) {
					return preset.Preset;
				}
			}
			return QualitySetting.Custom;
		}

		/*
		===============
		MatchesPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentValues"></param>
		/// <param name="preset"></param>
		/// <returns></returns>
		private static bool MatchesPreset( IReadOnlyDictionary<string, string> currentValues, SettingsPresetConfiguration preset ) {
			return preset.Settings.All( setting => currentValues.TryGetValue( setting.name, out var currentValue ) && currentValue == setting.value );
		}

		/*
		===============
		CreateLowQualityPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static SettingsPresetConfiguration CreateLowQualityPreset() {
			var settings = new CVarValue[] {
				new( "r.BakedLights", "false" ),
				new( "r.BloomEnabled", "true" ),
				new( "r.ForceVertexShading", "true" ),
				new( "r.PhysicallyBasedRendering", "false" )
			};

			return new SettingsPresetConfiguration( QualitySetting.Low, settings );
		}
	};
};