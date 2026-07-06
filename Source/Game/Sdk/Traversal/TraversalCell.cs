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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nomad.Game.Sdk.Traversal
{
    public readonly struct TraversalCell : IEquatable<TraversalCell>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        private readonly int _hashCode;

        public TraversalCell(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;

            unchecked
            {
                // Could we technically do HashCode.Combine? yeah but... nahhh
                _hashCode = (X * 73856093) ^ (Y * 19349663) ^ (Z * 83492791);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TraversalCell other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TraversalCell other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
