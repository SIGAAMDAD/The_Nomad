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
    /// <summary>
    /// Data-authored GOAP action definition.
    /// </summary>
    /// <remarks>
    /// <see cref="BehaviorId"/> follows the item-definition extension model.
    /// Static planner data lives here; executable behavior is provided by a
    /// registered module action behavior when the action needs custom runtime work.
    /// </remarks>
    public sealed record AgentActionDefinition
    {
        public AgentActionDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString? BehaviorId { get; init; }
        public int BaseCost { get; init; }
        public WorldCondition[] Preconditions { get; init; } = Array.Empty<WorldCondition>();
        public WorldEffect[] Effects { get; init; } = Array.Empty<WorldEffect>();

        public WorldStateMask PreconditionMask => WorldStateMask.FromConditions(Preconditions);
        public WorldStatePatch EffectPatch => WorldStatePatch.FromEffects(Effects);

        public PlannerAction Compile(
            IActionRunner runner,
            Func<PlanningContext, bool>? validateContext = null,
            Func<PlanningContext, int>? getDynamicCost = null
        )
        {
            if (runner == null)
            {
                throw new ArgumentNullException(nameof(runner));
            }

            return new PlannerAction(
                Id.ToString(),
                BaseCost,
                PreconditionMask,
                EffectPatch,
                runner,
                validateContext,
                getDynamicCost
            );
        }

        public static AgentActionDefinition Load(JsonElement json)
        {
            return new AgentActionDefinition
            {
                Id = new AgentActionDefinitionId(new InternString(JsonLoader.GetRequired<string>(json, nameof(Id)))),
                DisplayName = new InternString(JsonLoader.TryGet(json, nameof(DisplayName), out string displayName) ? displayName : string.Empty),
                BehaviorId = AgentDefinitionJson.LoadOptionalBehaviorId(json),
                BaseCost = JsonLoader.GetRequired<int>(json, nameof(BaseCost)),
                Preconditions = AgentDefinitionJson.LoadConditions(json, nameof(Preconditions)),
                Effects = AgentDefinitionJson.LoadEffects(json, nameof(Effects))
            };
        }
    }
}
