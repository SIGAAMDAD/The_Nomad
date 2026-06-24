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

namespace Nomad.Game.Sdk.Npc.Planner
{
    /// <summary>
    /// Overwrites specific bits in a <see cref="WorldState"/> with new <see cref="WorldKey"/> values.
    /// </summary>
    public readonly struct WorldStatePatch
    {
        public static WorldStatePatch Empty => new WorldStatePatch(0UL, 0UL, 0UL, 0UL);

        public readonly ulong LoMask;
        public readonly ulong LoValues;
        public readonly ulong HiMask;
        public readonly ulong HiValues;

        public WorldStatePatch(ulong loMask, ulong loValues, ulong hiMask, ulong hiValues)
        {
            LoMask = loMask;
            LoValues = loValues;
            HiMask = hiMask;
            HiValues = hiValues;
        }

        public static WorldStatePatch AddEffect(WorldStatePatch patch, WorldEffect effect)
        {
            ulong loMask = patch.LoMask;
            ulong loValues = patch.LoValues;
            ulong hiMask = patch.HiMask;
            ulong hiValues = patch.HiValues;

            SetPatchBit(
                effect.Key,
                effect.Value,
                ref loMask,
                ref loValues,
                ref hiMask,
                ref hiValues
            );

            return new WorldStatePatch(loMask, loValues, hiMask, hiValues);
        }

        public static WorldStatePatch FromEffects(ReadOnlySpan<WorldEffect> effects)
        {
            ulong loMask = 0UL;
            ulong loValues = 0UL;
            ulong hiMask = 0UL;
            ulong hiValues = 0UL;

            for (int i = 0; i < effects.Length; i++)
            {
                WorldEffect effect = effects[i];
                SetPatchBit(
                    effect.Key,
                    effect.Value,
                    ref loMask,
                    ref loValues,
                    ref hiMask,
                    ref hiValues
                );
            }

            return new WorldStatePatch(loMask, loValues, hiMask, hiValues);
        }

        private static void SetPatchBit(
            WorldKey key,
            bool requiredValue,
            ref ulong loMask,
            ref ulong loValues,
            ref ulong hiMask,
            ref ulong hiValues
        )
        {
            int index = (int)key;

            if ((uint)index >= 128U)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(key),
                    $"WorldKey '{key}' has index {index}, but WorldStatePatch only supports 128 keys."
                );
            }

            if (index < 64)
            {
                ulong bit = 1UL << index;
                ApplyPatchBit(key, requiredValue, bit, ref loMask, ref loValues);
                return;
            }

            int hiIndex = index - 64;
            ulong hiBit = 1UL << hiIndex;
            ApplyPatchBit(key, requiredValue, hiBit, ref hiMask, ref hiValues);
        }

        private static void ApplyPatchBit(
            WorldKey key,
            bool requiredValue,
            ulong bit,
            ref ulong mask,
            ref ulong values
        )
        {
            // already required?
            if ((mask & bit) != 0UL)
            {
                bool existingValue = (values & bit) != 0UL;

                if (existingValue != requiredValue)
                {
                    throw new InvalidOperationException(
                        $"Conflicting WorldStatePatch condition for '{key}'."
                    );
                }

                return;
            }

            mask |= bit;
            values = requiredValue
                ? values | bit
                : values & ~bit;
        }
    }
}
