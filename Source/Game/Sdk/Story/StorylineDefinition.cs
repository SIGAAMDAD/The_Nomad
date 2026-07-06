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
using System.Text.Json;
using Nomad.Core.Util;
using Nomad.Game.Sdk.Story.QuestTree;

namespace Nomad.Game.Sdk.Story
{
    public sealed record StorylineDefinition
    {
        public StorylineDefinitionId Id { get; init; }
        public InternString Description { get; init; }

        public IReadOnlyList<QuestDefinition> Quests { get; init; } = System.Array.Empty<QuestDefinition>();

        public QuestTreeDefinition? QuestTree { get; init; }

        public static StorylineDefinition Load(JsonElement json)
        {
            var questsJson = json.GetRequiredArray<JsonElement>(nameof(Quests));
            var quests = new List<QuestDefinition>(questsJson.Length);
            foreach (var quest in questsJson)
            {
                quests.Add(
                    QuestDefinition.Load(quest)
                );
            }

            return new StorylineDefinition
            {
                Id = new StorylineDefinitionId(new InternString(json.GetRequired<string>(nameof(Id)))),
                Description = new InternString(json.GetRequired<string>(nameof(Description))),
                Quests = quests
            };
        }
    }
}
