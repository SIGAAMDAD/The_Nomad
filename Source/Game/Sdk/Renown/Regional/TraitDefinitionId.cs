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
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Renown.Regional
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct TraitDefinitionId
    {
        public static readonly TraitDefinitionId Invalid = new TraitDefinitionId(InternString.Empty);

        public bool IsValid => Value != InternString.Empty;

        public readonly InternString Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TraitDefinitionId(InternString value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TraitDefinitionId other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return Value.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(TraitDefinitionId other)
        {
            return other.Value == Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator InternString(TraitDefinitionId value)
        {
            return value.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TraitDefinitionId left, TraitDefinitionId right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TraitDefinitionId left, TraitDefinitionId right)
        {
            return !left.Equals(right);
        }
    }
}
