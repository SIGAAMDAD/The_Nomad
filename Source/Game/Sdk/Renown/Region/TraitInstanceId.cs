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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nomad.Game.Sdk.Renown.Region
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct TraitInstanceId
    {
        public static readonly TraitInstanceId Invalid = new TraitInstanceId(-1);

        public readonly int Value;

        public bool IsValid => Value != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraitInstanceId(int value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TraitInstanceId other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TraitInstanceId other)
        {
            return other.Value == Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(TraitInstanceId value)
        {
            return value.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TraitInstanceId left, TraitInstanceId right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TraitInstanceId left, TraitInstanceId right)
        {
            return !left.Equals(right);
        }
    }
}
