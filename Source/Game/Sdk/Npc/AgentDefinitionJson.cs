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

namespace Nomad.Game.Sdk.Npc
{
    internal static class AgentDefinitionJson
    {
        public static InternString? LoadOptionalBehaviorId(JsonElement json)
        {
            if (!JsonLoader.TryGet(json, "BehaviorId", out string behaviorId))
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(behaviorId)
                ? null
                : new InternString(behaviorId);
        }

        public static AgentGoalDefinitionId[] LoadGoalReferences(JsonElement json, string propertyName)
        {
            if (!json.TryGetProperty(propertyName, out JsonElement referencesElement) ||
                referencesElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<AgentGoalDefinitionId>();
            }

            AgentGoalDefinitionId[] references = new AgentGoalDefinitionId[referencesElement.GetArrayLength()];
            int index = 0;

            foreach (JsonElement referenceElement in referencesElement.EnumerateArray())
            {
                references[index] = new AgentGoalDefinitionId(new InternString(referenceElement.GetString() ?? string.Empty));
                index++;
            }

            return references;
        }

        public static AgentActionDefinitionId[] LoadActionReferences(JsonElement json, string propertyName)
        {
            if (!json.TryGetProperty(propertyName, out JsonElement referencesElement) ||
                referencesElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<AgentActionDefinitionId>();
            }

            AgentActionDefinitionId[] references = new AgentActionDefinitionId[referencesElement.GetArrayLength()];
            int index = 0;

            foreach (JsonElement referenceElement in referencesElement.EnumerateArray())
            {
                references[index] = new AgentActionDefinitionId(new InternString(referenceElement.GetString() ?? string.Empty));
                index++;
            }

            return references;
        }

        public static AgentSensorDefinitionId[] LoadSensorReferences(JsonElement json, string propertyName)
        {
            if (!json.TryGetProperty(propertyName, out JsonElement referencesElement) ||
                referencesElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<AgentSensorDefinitionId>();
            }

            AgentSensorDefinitionId[] references = new AgentSensorDefinitionId[referencesElement.GetArrayLength()];
            int index = 0;

            foreach (JsonElement referenceElement in referencesElement.EnumerateArray())
            {
                references[index] = new AgentSensorDefinitionId(new InternString(referenceElement.GetString() ?? string.Empty));
                index++;
            }

            return references;
        }

        public static WorldCondition[] LoadConditions(JsonElement json, string propertyName)
        {
            if (!json.TryGetProperty(propertyName, out JsonElement conditionsElement) ||
                conditionsElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<WorldCondition>();
            }

            WorldCondition[] conditions = new WorldCondition[conditionsElement.GetArrayLength()];
            int index = 0;

            foreach (JsonElement conditionElement in conditionsElement.EnumerateArray())
            {
                conditions[index] = new WorldCondition(
                    JsonLoader.GetRequired<WorldKey>(conditionElement, nameof(WorldCondition.Key)),
                    JsonLoader.GetRequired<bool>(conditionElement, nameof(WorldCondition.Value))
                );
                index++;
            }

            return conditions;
        }

        public static WorldEffect[] LoadEffects(JsonElement json, string propertyName)
        {
            if (!json.TryGetProperty(propertyName, out JsonElement effectsElement) ||
                effectsElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<WorldEffect>();
            }

            WorldEffect[] effects = new WorldEffect[effectsElement.GetArrayLength()];
            int index = 0;

            foreach (JsonElement effectElement in effectsElement.EnumerateArray())
            {
                effects[index] = new WorldEffect(
                    JsonLoader.GetRequired<WorldKey>(effectElement, nameof(WorldEffect.Key)),
                    JsonLoader.GetRequired<bool>(effectElement, nameof(WorldEffect.Value))
                );
                index++;
            }

            return effects;
        }
    }
}
