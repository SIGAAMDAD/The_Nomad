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
    ///
    /// </summary>
    public readonly struct ItemDefinitionId : IEquatable<ItemDefinitionId>
    {
        public static readonly ItemDefinitionId Invalid = new ItemDefinitionId(InternString.Empty);

        public readonly InternString Value;

        public bool IsValid => Value != InternString.Empty;

        public ItemDefinitionId(InternString value)
        {
            Value = value;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is ItemDefinitionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(ItemDefinitionId other)
        {
            return other.Value == Value;
        }

        public static implicit operator InternString(ItemDefinitionId value)
        {
            return value.Value;
        }

        public static bool operator ==(ItemDefinitionId left, ItemDefinitionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemDefinitionId left, ItemDefinitionId right)
        {
            return !left.Equals(right);
        }
    }
}
