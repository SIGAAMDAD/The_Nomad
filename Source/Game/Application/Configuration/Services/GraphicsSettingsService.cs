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

using Game.Application.Common;
using Game.Application.Common.Interfaces;
using Game.Application.Common.Models;
using Game.Application.Configuration.Enums;
using Game.Domain.Configuration.Interfaces;
using Game.Infrastructure.Configuration;
using NomadCore.Abstractions.Services;
using System.Collections.Generic;

namespace Game.Application.Configuration.Services {
	/*
	===================================================================================
	
	GraphicsSettingsService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class GraphicsSettingsService : IGraphicsSettingsService {
		private class EffectsConfiguration( GraphicsSettingsService service ) : IEffectsConfiguration {
			private readonly GraphicsSettingsService _service = service;

			public AnimationQuality AnimationQuality {
				get => _service._config.Effects.AnimationQuality;
				set => _service._config = _service._config with {
					Effects = _service._config.Effects with { AnimationQuality = value }
				};
			}
			public ParticleQuality ParticleQuality {
				get => _service._config.Effects.ParticleQuality;
				set => _service._config = _service._config with {
					Effects = _service._config.Effects with { ParticleQuality = value }
				};
			}

			public void Set( EffectsConfig config ) {
				_service._config.Effects = config;
			}
		};
		private class LightingConfiguration( GraphicsSettingsService service ) : ILightingConfiguration {
			private GraphicsSettingsService _service = service;

			public bool BakedLights {
				get => _service._config.Lighting.BakedLights;
				set => _service._config = _service._config with {
					Lighting = _service._config.Lighting with { BakedLights = value }
				};
			}
			public bool PhysicallyBasedRendering {
				get => _service._config.Lighting.PhysicallyBasedRendering;
				set => _service._config = _service._config with {
					Lighting = _service._config.Lighting with { PhysicallyBasedRendering = value }
				};
			}
			public bool BloomEnabled {
				get => _service._config.Lighting.BloomEnabled;
				set => _service._config = _service._config with {
					Lighting = _service._config.Lighting with { BloomEnabled = value }
				};
			}
			public bool ForceVertexShading {
				get => _service._config.Lighting.ForceVertexShading;
				set => _service._config = _service._config with {
					Lighting = _service._config.Lighting with { ForceVertexShading = value }
				};
			}

			public void Set( LightingConfig config ) {
				_service._config.Lighting = config;
			}
		};
		private class ShadowConfiguration( GraphicsSettingsService service ) : IShadowConfiguration {
			private GraphicsSettingsService _service = service;

			public ShadowFilterQuality ShadowFilterType {
				get => _service._config.Shadows.ShadowFilterType;
				set => _service._config = _service._config with {
					Shadows = _service._config.Shadows with { ShadowFilterType = value }
				};
			}
			public ShadowAtlasSize ShadowAtlasSize {
				get => _service._config.Shadows.ShadowAtlasSize;
				set => _service._config = _service._config with {
					Shadows = _service._config.Shadows with { ShadowAtlasSize = value }
				};
			}
			public float ShadowFilterSmooth {
				get => _service._config.Shadows.ShadowFilterSmooth;
				set => _service._config = _service._config with {
					Shadows = _service._config.Shadows with { ShadowFilterSmooth = value }
				};
			}

			public void Set( ShadowConfig config ) {
				_service._config.Shadows = config;
			}
		};

		private readonly IReadOnlyList<string> _shadowAtlasSizes = [
			ShadowAtlasSize.Size1024.ToDisplayString(),
			ShadowAtlasSize.Size2048.ToDisplayString(),
			ShadowAtlasSize.Size4096.ToDisplayString(),
			ShadowAtlasSize.Size8192.ToDisplayString()
		];
		private readonly IReadOnlyList<string> _effectsQualities = [
			TranslationKeys.Graphics.QualityLow,
			TranslationKeys.Graphics.QualityNormal,
			TranslationKeys.Graphics.QualityHigh
		];
		private readonly IReadOnlyList<string> _lightingQualities = [
			TranslationKeys.Graphics.QualityLow,
			TranslationKeys.Graphics.QualityNormal,
			TranslationKeys.Graphics.QualityHigh
		];
		private readonly IReadOnlyList<string> _shadowQualities = [
			TranslationKeys.Graphics.ShadowFilterTypeOff,
			TranslationKeys.Graphics.ShadowFilterTypeHard,
			TranslationKeys.Graphics.ShadowFilterTypeSoft
		];

		public IEffectsConfiguration Effects => _effects;
		private readonly EffectsConfiguration _effects;

		public ILightingConfiguration Lighting => _lighting;
		private readonly LightingConfiguration _lighting;

		public IShadowConfiguration Shadow => _shadow;
		private readonly ShadowConfiguration _shadow;

		private GraphicsConfig _config;
		private EffectsConfig _effectsConfig;
		private LightingConfig _lightingConfig;

		private readonly ICVarSystemService _cvarSystem;
		private readonly IGraphicsPresetRepository _presetRepository;

		/*
		===============
		GraphicsSettingsService
		===============
		*/
		public GraphicsSettingsService( ICVarSystemService cvarSystem ) {
			_cvarSystem = cvarSystem;
			_presetRepository = new GraphicsPresetRepository();

			_effects = new EffectsConfiguration( this );
			_lighting = new LightingConfiguration( this );
			_shadow = new ShadowConfiguration( this );

			_config = new GraphicsConfig() {
				Effects = new EffectsConfig() {
					ParticleQuality = _cvarSystem.GetCVar<ParticleQuality>( "r.ParticleQuality" ).Value,
					AnimationQuality = _cvarSystem.GetCVar<AnimationQuality>( "r.AnimationQuality" ).Value
				},
				Lighting = new LightingConfig() {
					BakedLights = _cvarSystem.GetCVar<bool>( "r.BakedLights" ).Value,
					ForceVertexShading = _cvarSystem.GetCVar<bool>( "r.ForceVertexShading" ).Value,
					PhysicallyBasedRendering = _cvarSystem.GetCVar<bool>( "r.PhysicallyBasedRendering" ).Value,
					BloomEnabled = _cvarSystem.GetCVar<bool>( "r.BloomEnabled" ).Value
				},
				Shadows = new ShadowConfig() {
					ShadowFilterType = _cvarSystem.GetCVar<ShadowFilterQuality>( "r.ShadowFilterType" ).Value,
					ShadowAtlasSize = _cvarSystem.GetCVar<ShadowAtlasSize>( "r.ShadowAtlasSize" ).Value,
					ShadowFilterSmooth = _cvarSystem.GetCVar<float>( "r.ShadowFilterSmooth" ).Value
				}
			};
		}

		/*
		===============
		GetCurrentConfig
		===============
		*/
		public GraphicsConfig GetCurrentConfig() {
			return _config;
		}

		/*
		===============
		GetShadowAtlasSizes
		===============
		*/
		public IReadOnlyList<string> GetShadowAtlasSizes() {
			return _shadowAtlasSizes;
		}

		/*
		===============
		GetEffectsQualityList
		===============
		*/
		public IReadOnlyList<string> GetEffectsQualityList() {
			return _effectsQualities;
		}

		/*
		===============
		GetLightingQualityList
		===============
		*/
		public IReadOnlyList<string> GetLightingQualityList() {
			return _lightingQualities;
		}

		/*
		===============
		GetShadowFilterQualityList
		===============
		*/
		public IReadOnlyList<string> GetShadowFilterQualityList() {
			return _shadowQualities;
		}

		/*
		===============
		SetPreset
		===============
		*/
		public void SetPreset( QualitySetting preset ) {
		}

		/*
		===============
		GetPreset
		===============
		*/
		public QualitySetting GetPreset() {
			return _presetRepository.DetectPreset( _config );
		}

		/*
		===============
		GetEffectsPreset
		===============
		*/
		public QualitySetting GetEffectsPreset() {
			return _presetRepository.DetectEffectsPreset( _effectsConfig );
		}

		/*
		===============
		GetLightingPreset
		===============
		*/
		public QualitySetting GetLightingPreset() {
			return _presetRepository.DetectLightingPreset( _lightingConfig );
		}

		/*
		===============
		GetLightingPreset
		===============
		*/
		public QualitySetting GetShadowPreset() {
			return _presetRepository.DetectLightingPreset( _lightingConfig );
		}
	};
};