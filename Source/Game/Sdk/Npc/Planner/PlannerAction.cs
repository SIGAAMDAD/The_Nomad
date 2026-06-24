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
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Npc.Planner
{
    public sealed class PlannerAction
    {
        public InternString Name { get; }
        public int BaseCost { get; }
        public WorldStateMask Preconditions { get; }
        public WorldStatePatch Effects { get; }
        public Func<PlanningContext, bool> ValidateContext { get; }
        public Func<PlanningContext, int> GetDynamicCost { get; }
        public IActionRunner Runner { get; }

        public PlannerAction(string name, int baseCost, WorldStateMask preconditions, WorldStatePatch effects, IActionRunner runner, Func<PlanningContext, bool> validateContext = null, Func<PlanningContext, int> getDynamicCost = null)
        {
            Name = new InternString(name);
            BaseCost = baseCost;
            Preconditions = preconditions;
            Effects = effects;
            Runner = runner;
            ValidateContext = validateContext;
            GetDynamicCost = getDynamicCost;
        }
    }
}
