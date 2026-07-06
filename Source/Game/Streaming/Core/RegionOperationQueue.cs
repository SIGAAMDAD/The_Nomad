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

using Nomad.Core.Compatibility.Guards;

namespace Nomad.Game.Streaming
{
	internal sealed class RegionOperationQueue
	{
		private readonly ushort[] _items;
		private int _head = 0;
		private int _tail = 0;
		private int _count = 0;

		public int Count => _count;
		public int Capacity => _items.Length;

		public RegionOperationQueue( int capacity )
		{
			RangeGuard.ThrowIfNegative( capacity, nameof( capacity ) );

			_items = new ushort[capacity];
		}

		public void Clear()
		{
			_head = 0;
			_tail = 0;
			_count = 0;
		}

		public bool TryEnqueue( ushort index )
		{
			if ( _count == _items.Length ) {
				return false;
			}

			_items[_tail] = index;
			_tail++;
			if ( _tail == _items.Length ) {
				_tail = 0;
			}

			_count++;
			return true;
		}

		public bool TryDequeue( out ushort index )
		{
			if ( _count == 0 ) {
				index = 0;
				return false;
			}

			index = _items[_head];
			_head++;
			if ( _head == _items.Length ) {
				_head = 0;
			}

			_count--;
			return true;
		}
	};
};
