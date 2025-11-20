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
using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Menus.Settings {
	/*
	===================================================================================
	
	ConfigHandler
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public class ConfigHandler {
		public object? this[ string configName ] {
			get {
				foreach ( var cvar in Cvars ) {
					if ( cvar.Key.Name == configName ) {
						return cvar.Value;
					}
				}
				return null;
			}
			set {
				ArgumentNullException.ThrowIfNull( value );

				foreach ( var cvar in Cvars ) {
					if ( cvar.Key.Name == configName ) {
						Cvars[ cvar.Key ] = value;
						return;
					}
				}
				throw new KeyNotFoundException( configName );
			}
		}

		public virtual Dictionary<ICVar, object>? Cvars { get; }

		/*
		===============
		ConfigHandler
		===============
		*/
		public ConfigHandler() {
			ArgumentNullException.ThrowIfNull( Cvars );
		}

		/*
		===============
		ConfigHandlers
		===============
		*/
		protected ConfigHandler( in IReadOnlyDictionary<ICVar, object> cvars ) {
			ArgumentNullException.ThrowIfNull( cvars );
			Cvars = (Dictionary<ICVar, object>?)cvars;
		}

		/*
		===============
		Save
		===============
		*/
		public void Save() {
			ArgumentNullException.ThrowIfNull( Cvars );

			foreach ( var cvar in Cvars ) {
				cvar.Key.SetFromString( cvar.Value.ToString() );
			}
		}

		/*
		===============
		Load
		===============
		*/
		public void Load() {
			ArgumentNullException.ThrowIfNull( Cvars );

			foreach ( var cvar in Cvars ) {
				Cvars[ cvar.Key ] = cvar.Key.Value;
			}
		}

		/*
		===============
		GetHashCode
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override int GetHashCode() {
			return base.GetHashCode();
		}

		/*
		===============
		Equals
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals( object? obj ) {
			ArgumentNullException.ThrowIfNull( Cvars );
			ArgumentNullException.ThrowIfNull( obj );

			if ( obj is ConfigHandler config ) {
				ArgumentNullException.ThrowIfNull( config.Cvars );
				foreach ( var cvar in Cvars ) {
					if ( !config.Cvars.TryGetValue( cvar.Key, out object? value ) ) {
						return false;
					}
					if ( !cvar.Value.Equals( value ) ) {
						return false;
					}
				}
				return true;
			}
			return false;
		}
	};
};