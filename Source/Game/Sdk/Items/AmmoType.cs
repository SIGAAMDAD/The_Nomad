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
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Items
{
    /// <summary>
    /// Identifies a data-loaded cartridge or payload family, such as
    /// <c>9mm</c>, <c>45ACP</c>, <c>7.62</c>, <c>50BMG</c>,
    /// <c>12Gauge</c>, or <c>20Gauge</c>.
    /// </summary>
    /// <remarks>
    /// This is intentionally a value object instead of an enum. Mods and data
    /// packs can add granular ammunition families without requiring a code
    /// change. Fixed gameplay buckets live on <see cref="AmmoCategory"/> and
    /// are selected by each loaded <see cref="AmmoDefinition"/>.
    /// </remarks>
    public readonly struct AmmoType : IEquatable<AmmoType>
    {
        public static readonly AmmoType Invalid = new AmmoType(InternString.Empty);

        public readonly InternString Value;

        public bool IsValid => Value != InternString.Empty;

        public AmmoType(InternString value)
        {
            Value = value;
        }

        public AmmoType(string value)
            : this(new InternString(value))
        {
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is AmmoType other && Equals(other);
        }

        public bool Equals(AmmoType other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString() ?? string.Empty;
        }

        public static bool operator ==(AmmoType left, AmmoType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AmmoType left, AmmoType right)
        {
            return !left.Equals(right);
        }
    }
}
