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

namespace Nomad.Game.Sdk.Story.QuestTree
{
    public sealed record QuestTreeDefinition
    {
        public StorylineDefinitionId StorylineId { get; init; } = StorylineDefinitionId.Invalid;

        public IReadOnlyList<QuestNode> Nodes { get; init; } =
            Array.Empty<QuestNode>();

        public IReadOnlyList<QuestTreeEdge> Edges { get; init; } =
            Array.Empty<QuestTreeEdge>();

        public IReadOnlyList<QuestConditionNode> Conditions { get; init; } =
            Array.Empty<QuestConditionNode>();

        public IReadOnlyList<QuestDefinitionId> EntryQuests { get; init; } =
            Array.Empty<QuestDefinitionId>();
    }
}
