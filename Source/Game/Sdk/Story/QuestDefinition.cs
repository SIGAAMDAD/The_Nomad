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
using System.Collections.Generic;
using System.Text.Json;
using Nomad.Core.Util;

namespace Nomad.Game.Sdk.Story
{
    public sealed record QuestDefinition
    {
        public QuestDefinitionId Id { get; init; }
        public InternString DisplayName { get; init; }
        public InternString Description { get; init; }

        public IReadOnlyList<QuestObjectiveDefinition> Objectives { get; init; }
            = Array.Empty<QuestObjectiveDefinition>();

        public static QuestDefinition Load(JsonElement json)
        {
            var objectivesJson = json.GetRequiredArray<JsonElement>(nameof(Objectives));
            var objectives = new List<QuestObjectiveDefinition>(objectivesJson.Length);

            foreach (var objective in objectivesJson)
            {
                objectives.Add(
                    QuestObjectiveDefinition.Load(objective)
                );
            }

            return new QuestDefinition
            {
                Id = new QuestDefinitionId(new InternString(json.GetRequired<string>(nameof(Id)))),
                DisplayName = new InternString(json.GetRequired<string>(nameof(DisplayName))),
                Description = new InternString(json.GetRequired<string>(nameof(Description))),
                Objectives = objectives
            };
        }
    }
}
