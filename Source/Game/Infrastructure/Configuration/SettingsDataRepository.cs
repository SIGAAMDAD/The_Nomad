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

using Game.Domain.Configuration.Interfaces;
using Nomad.Core.Exceptions;
using Nomad.CVars;
using System;
using System.Collections.Generic;

namespace Game.Infrastructure.Configuration {
	/*
	===================================================================================
	
	SettingsDataRepository
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class SettingsDataRepository : ISettingsDataRepository {
		private readonly ICVarSystemService _cvarSystem;

		/*
		===============
		SettingsDataRepository
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cvarSystem"></param>
		public SettingsDataRepository( ICVarSystemService cvarSystem ) {
			ArgumentNullException.ThrowIfNull( cvarSystem );

			_cvarSystem = cvarSystem;
		}

		/*
		===============
		GetValue
		===============
		*/
		/// <summary>
		/// Retrieves a cvar value from the cvar database.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		public T GetValue<T>( string name ) {
			var cvar = _cvarSystem.GetCVar<T>( name ) ?? throw new CVarMissing( name );
			return cvar.Value;
		}

		/*
		===============
		SetValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The cvar's name.</param>
		/// <param name="value">The value to assign to the cvar.</param>
		public void SetValue<T>( string name, T value ) {
			var cvar = _cvarSystem.GetCVar<T>( name ) ?? throw new CVarMissing( name );
			cvar.Value = value;
		}

		/*
		===============
		HasValue
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool HasValue<T>( string name ) {
			return _cvarSystem.GetCVar<T>( name ) != null;
		}

		/*
		===============
		ResetValues
		===============
		*/
		/// <summary>
		/// Resets all cvars in the group to their default values.
		/// </summary>
		public void ResetValues() {
			var cvars = _cvarSystem.GetCVars();
			foreach ( var cvar in cvars ) {
				cvar.Reset();
			}
		}
	};
};