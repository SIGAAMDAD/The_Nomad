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

using Game.Application.Common.Models.ValueObjects;
using Game.Application.Configuration.Enums;
using Game.Domain.Configuration.Enums;
using Game.Domain.Configuration.Interfaces;
using System.Collections.Generic;

namespace Game.Infrastructure.Configuration {
	/*
	===================================================================================
	
	GraphicsPresetRepository
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public class GraphicsPresetRepository : IGraphicsPresetRepository {
		private readonly Dictionary<QualitySetting, GraphicsConfig> _presets;
		private readonly Dictionary<QualitySetting, EffectsConfig> _effectsPresets;
		private readonly Dictionary<QualitySetting, LightingConfig> _lightingPresets;

		public GraphicsPresetRepository() {
			_effectsPresets = new Dictionary<QualitySetting, EffectsConfig>() {
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
		public GraphicsConfig GetPreset( QualitySetting preset ) {
			return _presets[ preset ];
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
		public EffectsConfig GetEffectsPreset( QualitySetting preset ) {
			return _effectsPresets[ preset ];
		}

		/*
		===============
		GetLightingPreset
		===============
		*/
		public LightingConfig GetLightingPreset( QualitySetting preset ) {
			return _lightingPresets[ preset ];
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
		public QualitySetting DetectPreset( GraphicsConfig currentValues ) {
			foreach ( var preset in _presets ) {
				if ( MatchesPreset( currentValues, preset.Value ) ) {
					return preset.Key;
				}
			}
			return QualitySetting.Custom;
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
		public QualitySetting DetectEffectsPreset( EffectsConfig currentValues ) {
			foreach ( var preset in _effectsPresets ) {
				if ( MatchesPreset( currentValues, preset.Value ) ) {
					return preset.Key;
				}
			}
			return QualitySetting.Custom;
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
		/// <returns></returns>LightingConfig
		public QualitySetting DetectLightingPreset( LightingConfig currentValues ) {
			foreach ( var preset in _lightingPresets ) {
				if ( MatchesPreset( currentValues, preset.Value ) ) {
					return preset.Key;
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
		private static bool MatchesPreset( GraphicsConfig currentValues, GraphicsConfig preset ) {
			return preset == currentValues;
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
		private static bool MatchesPreset( EffectsConfig currentValues, EffectsConfig preset ) {
			return preset == currentValues;
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
		private static bool MatchesPreset( LightingConfig currentValues, LightingConfig preset ) {
			return preset == currentValues;
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
		public static EffectsConfig CreateLowQualityPreset() {
			return new EffectsConfig() {
				AnimationQuality = AnimationQuality.Low,
				ParticleQuality = ParticleQuality.Low
			};
		}

		/*
		===============
		CreateNormalQualityPreset
		===============
		*/
		public static EffectsConfig CreateNormalQualityPreset() {
			return new EffectsConfig() {
				AnimationQuality = AnimationQuality.Medium,
				ParticleQuality = ParticleQuality.Low
			};
		}

		/*
		===============
		CreateHighQualityPreset
		===============
		*/
		public static EffectsConfig CreateHighQualityPreset() {
			return new EffectsConfig() {
				AnimationQuality = AnimationQuality.High,
				ParticleQuality = ParticleQuality.High,
			};
		}

		public ShadowConfig GetShadowPreset( QualitySetting preset ) {
			throw new System.NotImplementedException();
		}

		public QualitySetting DetectShadowPreset( ShadowConfig currentValues ) {
			throw new System.NotImplementedException();
		}
	};
};