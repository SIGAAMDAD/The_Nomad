/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til

This Source Code Form is subject to the terms of the Mozilla Public
License, v2. If a copy of the MPL was not distributed with this
file, You can obtain one at https://mozilla.org/MPL/2.0/.

This software is provided "as is", without warranty of any kind,
express or implied, including but not limited to the warranties
of merchantability, fitness for a particular purpose and noninfringement.
===========================================================================
*/

using System;
using System.Runtime.CompilerServices;
using Nomad.Core.Collections;
using Nomad.Core.Util;

namespace Nomad.Game.Gameplay.Story.Runtime
{
	internal sealed class InternStringLocalIdTable
	{
		private int[] _globalToLocal;
		private InternString[] _localToGlobal;
		private int _count;

		public int Count {
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get {
				return _count;
			}
		}

		public InternStringLocalIdTable( int initialGlobalCapacity, int initialLocalCapacity )
		{
			if ( initialGlobalCapacity < 1 ) {
				initialGlobalCapacity = 1;
			}

			if ( initialLocalCapacity < 1 ) {
				initialLocalCapacity = 1;
			}

			_globalToLocal = new int[initialGlobalCapacity];
			_localToGlobal = new InternString[initialLocalCapacity];
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public InternString GetGlobal( int localId )
		{
			return _localToGlobal[localId];
		}

		public int Register( InternString key )
		{
			ulong raw = key;
			EnsureGlobalCapacity( raw );

			int encoded = _globalToLocal[(int)raw];
			if ( encoded != 0 ) {
				return encoded - 1;
			}

			int local = _count;
			_count++;

			EnsureLocalCapacity( _count );

			_globalToLocal[(int)raw] = local + 1;
			_localToGlobal[local] = key;

			return local;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool TryResolve( InternString key, out int localId )
		{
			ulong raw = key;

			if ( raw >= (ulong)_globalToLocal.Length ) {
				localId = -1;
				return false;
			}

			int encoded = _globalToLocal[(int)raw];
			if ( encoded == 0 ) {
				localId = -1;
				return false;
			}

			localId = encoded - 1;
			return true;
		}

		private void EnsureGlobalCapacity( ulong raw )
		{
			if ( raw < (ulong)_globalToLocal.Length ) {
				return;
			}

			if ( raw > int.MaxValue - 1 ) {
				throw new InvalidOperationException( "InternString id exceeds local resolver capacity." );
			}

			int required = (int)raw + 1;
			int newCapacity = CollectionMath.NextPowerOfTwo( required );

			Array.Resize( ref _globalToLocal, newCapacity );
		}

		private void EnsureLocalCapacity( int required )
		{
			if ( required <= _localToGlobal.Length ) {
				return;
			}

			int newCapacity = CollectionMath.NextPowerOfTwo( required );

			Array.Resize( ref _localToGlobal, newCapacity );
		}
	};
};
