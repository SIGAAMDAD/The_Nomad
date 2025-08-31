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

using System;
using System.Runtime.CompilerServices;

namespace Util {
	/*
	===================================================================================

	CVar

	===================================================================================
	*/
	/// <summary>
	/// Literally just a Quake 3/UE5 style CVar but without the copyright issues.
	/// A configurable variable that can be set from the console and settings.ini
	/// </summary>

	public struct CVar<T> {
		public readonly string Name { get; }
		public readonly string Description { get; }
		public readonly float Min { get; }
		public readonly float Max { get; }
		public T Value { get; private set; }

		private readonly T DefaultValue;

		/*
		===============
		CVar
		===============
		*/
		/// <summary>
		/// Constructs a cvar
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <param name="description"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is invalid as a CVar name</exception>
		public CVar( string name, T value, string description = "", float min = 0.0f, float max = 0.0f ) {
			ArgumentException.ThrowIfNullOrEmpty( name );

			if ( !IsValidName( name ) ) {
				throw new ArgumentException( $"CVar name {name} contains invalid characters" );
			}

			Name = name;
			Value = value;

			Min = min;
			Max = max;

			Description = description;

			DefaultValue = value;
		}

		/*
		===============
		IsValidNameCharacter
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsValidNameCharacter( char c ) {
			return !( c == '\\' || c == '\"' || c == ';' || c == '%' || c <= ' ' || c >= '~' );
		}

		/*
		===============
		IsValidName
		===============
		*/
		/// <summary>
		/// Ensures the provided <paramref name="cvarName"/> string doesn't contain any invalid non alphanumeric characters
		/// </summary>
		/// <param name="cvarName">The name to check</param>
		/// <returns>Returns true if the name is valid</returns>
		private static bool IsValidName( string cvarName ) {
			for ( int i = 0; i < cvarName.Length; i++ ) {
				if ( !IsValidNameCharacter( cvarName[ i ] ) ) {
					return false;
				}
			}
			return true;
		}

		public void Reset() {
			Value = DefaultValue;
		}

		/*
		===============
		Set
		===============
		*/
		/// <summary>
		/// Sets the cvar's value
		/// </summary>
		/// <param name="newValue">The value to set the cvar to</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Set( T newValue ) {
			Value = newValue;
		}
	};
};