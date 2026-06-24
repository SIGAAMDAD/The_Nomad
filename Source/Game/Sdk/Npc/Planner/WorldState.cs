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

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Nomad.Game.Sdk.Npc.Planner
{
    public readonly struct WorldState
    {
        private readonly ulong _lo;
        private readonly ulong _hi;

        public WorldState(ulong lo, ulong hi)
        {
            _lo = lo;
            _hi = hi;
        }

        public bool Get(WorldKey key)
        {
            int index = (int)key;
            if (index < 64)
            {
                ulong mask = 1UL << index;
                return (_lo & mask) != 0;
            }

            index -= 64;
            ulong hiMask = 1UL << index;
            return (_hi & hiMask) != 0;
        }

        public WorldState Set(WorldKey key, bool value)
        {
            int index = (int)key;
            ulong lo = _lo;
            ulong hi = _hi;

            if (index < 64)
            {
                ulong mask = 1UL << index;

                lo = value
                    ? lo | mask
                    : lo & ~mask;
            }
            else
            {
                index -= 64;
                ulong mask = 1UL << index;
                hi = value
                    ? hi | mask
                    : hi & ~mask;
            }

            return new WorldState(lo, hi);
        }

        public bool Meets(WorldCondition[] conditions)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (Get(conditions[i].Key) != conditions[i].Value)
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Meets(WorldStateMask mask)
        {
            return (((_lo ^ mask.LoValues) & mask.LoMask) == 0L)
                && (((_hi ^ mask.HiValues) & mask.HiMask) == 0L);
        }

        public WorldState Apply(WorldEffect[] effects)
        {
            WorldState state = this;

            for (int i = 0; i < effects.Length; i++)
            {
                state = state.Set(effects[i].Key, effects[i].Value);
            }

            return state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldState Apply(WorldStatePatch patch)
        {
            ulong lo = (_lo & ~patch.LoMask) | (patch.LoValues & patch.LoMask);
            ulong hi = (_hi & ~patch.HiMask) | (patch.HiValues & patch.HiMask);

            return new WorldState(lo, hi);
        }

        public int CountUnmet(WorldCondition[] conditions)
        {
            int count = 0;

            for (int i = 0; i < conditions.Length; i++)
            {
                if (Get(conditions[i].Key) != conditions[i].Value)
                {
                    count++;
                }
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CountUnmet(WorldStateMask mask)
        {
            ulong loUnmet = (_lo ^ mask.LoValues) & mask.LoMask;
            ulong hiUnmet = (_hi ^ mask.HiValues) & mask.HiMask;

            return BitOperations.PopCount(loUnmet) + BitOperations.PopCount(hiUnmet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(WorldState other)
        {
            return _lo == other._lo && _hi == other._hi;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is WorldState other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                return (_lo.GetHashCode() * 397) ^ _hi.GetHashCode();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WorldState left, WorldState right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WorldState left, WorldState right)
        {
            return !left.Equals(right);
        }
    }
}
