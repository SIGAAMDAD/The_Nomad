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
using System.Buffers;

namespace Nomad.Game.Application.Gameplay.Traversal
{
	internal sealed class TraversalQueryBuffer
	{
		public int[] Items;
		public int Count;

		public TraversalQueryBuffer( int capacity )
		{
			Items = ArrayPool<int>.Shared.Rent( Math.Max( capacity, 8 ) );
			Count = 0;
		}

		public void Clear()
		{
			Count = 0;
		}

		public void Add( int value )
		{
			if ( Count >= Items.Length ) {
				ArrayPool<int>.Shared.Return( Items );
				Items = ArrayPool<int>.Shared.Rent( Items.Length * 2 );
			}
			Items[Count++] = value;
		}
	}
}
