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
using Game.Application.Configuration.Enums;
using Game.Domain.Configuration.Interfaces;
using Game.Domain.Events;
using NomadCore.Abstractions.Services;
using System;

namespace Game.Application.Configuration {
	/*
	===================================================================================
	
	GraphicsPresetService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class GraphicsPresetService : IGraphicsPresetService {
		private readonly ICVarSystemService _cvarSystem;
		private readonly IGraphicsPresetRepository _presetRepository;
		private readonly IGraphicsSettingsService _graphicsService;

		public static readonly SettingsPresetApplied SettingsPresetApplied = new SettingsPresetApplied();

		/*
		===============
		GraphicsPresetService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		/// <param name="presetRepository"></param>
		public GraphicsPresetService( ICVarSystemService? cvarSystem, IGraphicsSettingsService? service, IGraphicsPresetRepository? presetRepository ) {
			ArgumentNullException.ThrowIfNull( cvarSystem );
			ArgumentNullException.ThrowIfNull( service );
			ArgumentNullException.ThrowIfNull( presetRepository );

			_graphicsService = service;
			_cvarSystem = cvarSystem;
			_presetRepository = presetRepository;
		}

		/*
		===============
		ApplyPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="preset"></param>
		public void ApplyPreset( QualitySetting preset ) {
			ApplyEffectsPreset( preset );
			ApplyLightingPreset( preset );
			ApplyShadowPreset( preset );

			SettingsPresetApplied.Publish( new SettingsPresetAppliedEventData( preset ) );
		}

		/*
		===============
		ApplyEffectsPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="preset"></param>
		public void ApplyEffectsPreset( QualitySetting preset ) {
			_graphicsService.Effects.Set( _presetRepository.GetEffectsPreset( preset ) );
		}

		/*
		===============
		ApplyLightingPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="preset"></param>
		public void ApplyLightingPreset( QualitySetting preset ) {
			_graphicsService.Lighting.Set( _presetRepository.GetLightingPreset( preset ) );
		}

		/*
		===============
		ApplyShadowPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="preset"></param>
		public void ApplyShadowPreset( QualitySetting preset ) {
			_graphicsService.Shadow.Set( _presetRepository.GetShadowPreset( preset ) );
		}

		/*
		===============
		DetectCurrentPreset
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public QualitySetting DetectCurrentPreset( GraphicsConfig config ) {
			return _presetRepository.DetectPreset( config );
		}

		/*
		===============
		DetectCurrentPreset
		===============
		*/
		public QualitySetting DetectCurrentPreset( EffectsConfig config ) {
			return _presetRepository.DetectEffectsPreset( config );
		}

		/*
		===============
		DetectCurrentPreset
		===============
		*/
		public QualitySetting DetectCurrentPreset( LightingConfig config ) {
			return _presetRepository.DetectLightingPreset( config );
		}

		/*
		===============
		DetectCurrentPreset
		===============
		*/
		public QualitySetting DetectCurrentPreset( ShadowConfig config ) {
			return _presetRepository.DetectShadowPreset( config );
		}
	};
};