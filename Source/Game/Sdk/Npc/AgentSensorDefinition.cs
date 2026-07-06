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

using System.Text.Json;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Npc
{
    /// <summary>
    /// Data-authored NPC sensor definition.
    /// </summary>
    public sealed record AgentSensorDefinition
    {
        public AgentSensorDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString? BehaviorId { get; init; }
        public float TickInterval { get; init; }
        public bool Enabled { get; init; } = true;

        public static AgentSensorDefinition Load(JsonElement json)
        {
            return new AgentSensorDefinition
            {
                Id = new AgentSensorDefinitionId(new InternString(JsonLoader.GetRequired<string>(json, nameof(Id)))),
                DisplayName = new InternString(JsonLoader.TryGet(json, nameof(DisplayName), out string displayName) ? displayName : string.Empty),
                BehaviorId = AgentDefinitionJson.LoadOptionalBehaviorId(json),
                TickInterval = JsonLoader.TryGet(json, nameof(TickInterval), out float tickInterval) ? tickInterval : 0.0f,
                Enabled = !JsonLoader.TryGet(json, nameof(Enabled), out bool enabled) || enabled
            };
        }
    }
}
