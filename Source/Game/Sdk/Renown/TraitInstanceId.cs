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

namespace Nomad.Game.Sdk.Renown
{
    /// <summary>
    ///
    /// </summary>
    public readonly struct TraitInstanceId
    {
        public static readonly TraitInstanceId Invalid = new TraitInstanceId(Guid.Empty);

        public bool IsValid => Value != Guid.Empty;

        public readonly Guid Value;

        public TraitInstanceId(Guid value)
        {
            Value = value;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is TraitInstanceId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(TraitInstanceId other)
        {
            return other.Value == Value;
        }

        public static implicit operator Guid(TraitInstanceId value)
        {
            return value.Value;
        }

        public static bool operator ==(TraitInstanceId left, TraitInstanceId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TraitInstanceId left, TraitInstanceId right)
        {
            return !left.Equals(right);
        }
    }
}
