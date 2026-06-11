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

namespace Nomad.Game.Sdk.Traversal
{
    public readonly struct TraversalCell : IEquatable<TraversalCell>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public TraversalCell(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(TraversalCell other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TraversalCell other && Equals(other);
        }

		public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Z;
                return hash;
            }
        }
    }
}
