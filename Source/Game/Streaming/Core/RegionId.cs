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

namespace Nomad.Game.Streaming
{
	internal readonly struct RegionId : IEquatable<RegionId>
	{
		public readonly int X;
		public readonly int Z;

		public RegionId( int x, int z )
		{
			X = x;
			Z = z;
		}

		public bool Equals( RegionId other )
		{
			return X == other.X && Z == other.Z;
		}

		public override bool Equals( object obj )
		{
			return obj is RegionId other && Equals( other );
		}

		public override int GetHashCode()
		{
			return HashCode.Combine( X, Z );
		}

		public override string ToString()
		{
			return $"({X}, {Z})";
		}

		public static bool operator ==( RegionId left, RegionId right )
		{
			return left.Equals( right );
		}

		public static bool operator !=( RegionId left, RegionId right )
		{
			return !left.Equals( right );
		}
	};
};
