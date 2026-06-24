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

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nomad.Game.Sdk.Npc.Planner.Actions
{
    public sealed class ActionBuilder
    {
        private readonly List<WorldCondition> _preconditions = new List<WorldCondition>();
        private readonly List<WorldEffect> _effects = new List<WorldEffect>();

        public int BaseCost { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Requires(WorldKey key, bool value)
        {
            _preconditions.Add(new WorldCondition(key, value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Effect(WorldKey key, bool value)
        {
            _effects.Add(new WorldEffect(key, value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldStateMask GetPreconditions()
        {
            return WorldStateMask.FromConditions(_preconditions.ToArray());
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldStatePatch GetEffects()
        {
            return WorldStatePatch.FromEffects(_effects.ToArray());
        }
    }
}
