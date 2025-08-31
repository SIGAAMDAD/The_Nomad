/*
===========================================================================
Copyright (C) 2023-2025 Noah Van Til

This file is part of The Nomad source code.

The Nomad source code is free software; you can redistribute it
and/or modify it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

The Nomad source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with The Nomad source code; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA
===========================================================================
*/

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Util {
	public struct ObjectPool<T> where T : class, new() {
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