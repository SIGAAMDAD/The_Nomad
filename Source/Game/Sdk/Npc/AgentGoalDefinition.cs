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
using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Npc.Planner;
using Nomad.Game.Sdk.Npc.Planner.Goals;

namespace Nomad.Game.Sdk.Npc
{
    /// <summary>
    /// Data-authored GOAP goal definition.
    /// </summary>
    /// <remarks>
    /// <see cref="BehaviorId"/> follows the same extension pattern as item definitions:
    /// null means plain data-only behavior, while a value points at a registered
    /// module behavior that can provide dynamic scoring or specialized goal logic.
    /// </remarks>
    public sealed record AgentGoalDefinition
    {
        public AgentGoalDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString? BehaviorId { get; init; }
        public int BasePriority { get; init; }
        public WorldCondition[] DesiredState { get; init; } = Array.Empty<WorldCondition>();

        public WorldStateMask DesiredMask => WorldStateMask.FromConditions(DesiredState);

        public GoalDef Compile(Func<PlanningContext, int>? scoreModifier = null)
        {
            return new GoalDef(
                Id.ToString(),
                BasePriority,
                DesiredState,
                scoreModifier
            );
        }

        public static AgentGoalDefinition Load(JsonElement json)
        {
            return new AgentGoalDefinition
            {
                Id = new AgentGoalDefinitionId(new InternString(JsonLoader.GetRequired<string>(json, nameof(Id)))),
                DisplayName = new InternString(JsonLoader.TryGet(json, nameof(DisplayName), out string displayName) ? displayName : string.Empty),
                BehaviorId = AgentDefinitionJson.LoadOptionalBehaviorId(json),
                BasePriority = JsonLoader.GetRequired<int>(json, nameof(BasePriority)),
                DesiredState = AgentDefinitionJson.LoadConditions(json, nameof(DesiredState))
            };
        }
    }
}
