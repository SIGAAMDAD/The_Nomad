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
using Game.Application.Configuration.Enums;
using Game.Domain.Configuration.Interfaces;
using Game.Domain.Events;
using NomadCore.Abstractions.Services;
using System;
using System.Collections.Generic;

namespace Game.Application.Configuration {
	/*
	===================================================================================
	
	SettingsPresetService
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class SettingsPresetService : ISettingsPresetService {
		private readonly ICVarSystemService _cvarSystem;
		private readonly ISettingsPresetRepository _presetRepository;

		public static readonly SettingsPresetApplied SettingsPresetApplied = new SettingsPresetApplied();

		/*
		===============
		SettingsPresetService
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		/// <param name="presetRepository"></param>
		public SettingsPresetService( ICVarSystemService? cvarSystem, ISettingsPresetRepository? presetRepository ) {
			ArgumentNullException.ThrowIfNull( cvarSystem );
			ArgumentNullException.ThrowIfNull( presetRepository );

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
			var presetConfig = _presetRepository.GetPreset( preset );

			foreach ( var setting in presetConfig.Settings ) {
				_cvarSystem.GetCVar( setting.name ).SetFromString( setting.value );
			}

			SettingsPresetApplied.Publish( new SettingsPresetAppliedEventData( preset ) );
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
		public QualitySetting DetectCurrentPreset( IReadOnlyDictionary<string, string> values ) {
			return _presetRepository.DetectPreset( values );
		}
	};
};