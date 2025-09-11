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

using Godot;
using System.Collections.Generic;
using System.Linq;

namespace GUIDE {
	public sealed class GUIDESet {
		private Dictionary<Variant, Variant> Values = new Dictionary<Variant, Variant>();

		/*
		===============
		Add
		===============
		*/
		/// <summary>
		/// Adds the gien value to the set.
		/// If the value is already in the set, it will not be added again
		/// </summary>
		/// <param name="value">The value to add to the set</param>
		public void Add( Variant value ) {
			Values[ value ] = value;
		}

		/*
		===============
		AddAll
		===============
		*/
		/// <summary>
		/// Adds all values in the given array to the set.
		/// If a value is already in the set, it will not be added again
		/// </summary>
		/// <param name="values">The values to add to the set</param>
		public void AddAll( Godot.Collections.Array values ) {
			for ( int i = 0; i < values.Count; i++ ) {
				Values[ values[ i ] ] = values[ i ];
			}
		}

		/*
		===============
		Remove
		===============
		*/
		/// <summary>
		/// REmoves the given value from the set
		/// </summary>
		/// <param name="value">The value to remove from the set</param>
		public void Remove( Variant value ) {
			Values.Remove( value );
		}

		/*
		===============
		Clear
		===============
		*/
		/// <summary>
		/// Removes all values from the set
		/// </summary>
		public void Clear() {
			Values.Clear();
		}

		/*
		===============
		IsEmpty
		===============
		*/
		/// <summary>
		/// Returns true if the set is empty, false otherwise
		/// </summary>
		/// <returns></returns>
		public bool IsEmpty() {
			return Values.Count == 0;
		}

		/*
		===============
		Pull
		===============
		*/
		/// <summary>
		/// Returns the first item in the set and removes it from the set.
		/// If the set is empty, returns null
		/// </summary>
		/// <returns>The value extracted from the set</returns>
		public Variant Pull() {
			if ( IsEmpty() ) {
				return Variant.From<GodotObject>( null );
			}

			Variant key = Values.Keys.First();
			Remove( key );
			return key;
		}

		/*
		===============
		Has
		===============
		*/
		/// <summary>
		/// Checks whether the set contains the given value
		/// </summary>
		/// <param name="value">The value to check for</param>
		/// <returns></returns>
		public bool Has( Variant value ) {
			return Values.ContainsKey( value );
		}
	};
};