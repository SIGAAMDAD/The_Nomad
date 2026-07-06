/*
===========================================================================
The Nomad MPLv2 Source Code
Copyright (C) 2025-2026 Noah Van Til
===========================================================================
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Biomes;

namespace Nomad.Game.Sdk.World
{
    /// <summary>
    /// Compatibility identifier. Biome definitions now live in Nomad.Game.Sdk.Biomes.
    /// </summary>
    [Obsolete("Use Nomad.Game.Sdk.Biomes.BiomeDefinitionId.")]
    public readonly struct BiomeDefinitionId
    {
        public static readonly BiomeDefinitionId Invalid = new BiomeDefinitionId(InternString.Empty);

        public bool IsValid => Value != InternString.Empty;

        public readonly InternString Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiomeDefinitionId(InternString value)
        {
            Value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BiomeDefinitionId(Nomad.Game.Sdk.Biomes.BiomeDefinitionId biomeId)
        {
            Value = biomeId.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is BiomeDefinitionId other && Equals(other);
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
        public bool Equals(BiomeDefinitionId other)
        {
            return other.Value == Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Nomad.Game.Sdk.Biomes.BiomeDefinitionId ToBiomeDefinitionId()
        {
            return new Nomad.Game.Sdk.Biomes.BiomeDefinitionId(Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator InternString(BiomeDefinitionId value)
        {
            return value.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Nomad.Game.Sdk.Biomes.BiomeDefinitionId(BiomeDefinitionId value)
        {
            return new Nomad.Game.Sdk.Biomes.BiomeDefinitionId(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BiomeDefinitionId(Nomad.Game.Sdk.Biomes.BiomeDefinitionId value)
        {
            return new BiomeDefinitionId(value.Value);
        }
    }
}
