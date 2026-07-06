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

namespace Nomad.Game.Sdk.Npc
{
    /// <summary>
    /// Data-authored NPC agent archetype definition.
    /// </summary>
    /// <remarks>
    /// This is the world-database layer above the runtime planner archetype.
    /// Goals, actions, sensors, state compilers, selectors, and the optional
    /// top-level agent behavior are all referenced by interned ids so module
    /// content can extend behavior without hard-coding concrete C# types here.
    /// </remarks>
    public sealed record AgentDefinition
    {
        public AgentDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString? BehaviorId { get; init; }
        public InternString? StateCompilerBehaviorId { get; init; }
        public InternString? GoalSelectorBehaviorId { get; init; }
        public int PlanningBudget { get; init; } = 64;
        public AgentGoalDefinitionId[] Goals { get; init; } = Array.Empty<AgentGoalDefinitionId>();
        public AgentActionDefinitionId[] Actions { get; init; } = Array.Empty<AgentActionDefinitionId>();
        public AgentSensorDefinitionId[] Sensors { get; init; } = Array.Empty<AgentSensorDefinitionId>();

        public static AgentDefinition Load(JsonElement json)
        {
            return new AgentDefinition
            {
                Id = new AgentDefinitionId(new InternString(JsonLoader.GetRequired<string>(json, nameof(Id)))),
                DisplayName = new InternString(JsonLoader.TryGet(json, nameof(DisplayName), out string displayName) ? displayName : string.Empty),
                BehaviorId = AgentDefinitionJson.LoadOptionalBehaviorId(json),
                StateCompilerBehaviorId = LoadOptionalInternString(json, nameof(StateCompilerBehaviorId)),
                GoalSelectorBehaviorId = LoadOptionalInternString(json, nameof(GoalSelectorBehaviorId)),
                PlanningBudget = JsonLoader.TryGet(json, nameof(PlanningBudget), out int planningBudget) ? planningBudget : 64,
                Goals = AgentDefinitionJson.LoadGoalReferences(json, nameof(Goals)),
                Actions = AgentDefinitionJson.LoadActionReferences(json, nameof(Actions)),
                Sensors = AgentDefinitionJson.LoadSensorReferences(json, nameof(Sensors))
            };
        }

        private static InternString? LoadOptionalInternString(JsonElement json, string propertyName)
        {
            if (!JsonLoader.TryGet(json, propertyName, out string value))
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(value)
                ? null
                : new InternString(value);
        }
    }
}
