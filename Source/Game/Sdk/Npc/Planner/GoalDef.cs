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
using System.Runtime.CompilerServices;

namespace Nomad.Game.Sdk.Npc.Planner.Goals
{
    public sealed record GoalDef
    {
        public string Name { get; }
        public int BasePriority { get; }
        public WorldCondition[] DesiredState { get; }
        public Func<PlanningContext, int> ScoreModifier { get; }

        public GoalDef(string name, int basePriority, WorldCondition[] desiredState, Func<PlanningContext, int> scoreModifier = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BasePriority = basePriority;
            DesiredState = desiredState ?? Array.Empty<WorldCondition>();
            ScoreModifier = scoreModifier;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPriority(PlanningContext context)
        {
            return BasePriority + (ScoreModifier != null ? ScoreModifier(context) : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSatisfied(WorldState state)
        {
            return state.Meets(DesiredState);
        }
    }
}
