/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Renown.Regional
{
    public readonly struct RegionalRenownDefinitionId
    {
        public static readonly RegionalRenownDefinitionId Invalid = new RegionalRenownDefinitionId(InternString.Empty);

        public bool IsValid => Value != InternString.Empty;

        public readonly InternString Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RegionalRenownDefinitionId(InternString value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is RegionalRenownDefinitionId other && Equals(other);
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
        public bool Equals(RegionalRenownDefinitionId other)
        {
            return other.Value == Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator InternString(RegionalRenownDefinitionId value)
        {
            return value.Value;
        }
    }
}
