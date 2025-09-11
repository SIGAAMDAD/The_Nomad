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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Util {
	public struct ObjectPool<T> where T : new() {
		private readonly ConcurrentBag<T>? Pool;

		public ObjectPool() {
			Pool = new ConcurrentBag<T>();
		}

		/*
		===============
		Rent
		===============
		*/
		/// <summary>
		/// Returns an object from the ObjectPool. Allocates a new object if there's no free objects left in the pool
		/// </summary>
		/// <returns>An allocated object</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly T Rent() {
			if ( Pool.TryTake( out T? result ) ) {
				return result;
			}
			return new T();
		}

		/*
		===============
		Return
		===============
		*/
		/// <summary>
		/// Returns an object to the ObjectPool
		/// </summary>
		/// <param name="value"></param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly void Return( T value ) {
			Pool.Add( value );
		}
	};
};