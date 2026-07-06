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
using Nomad.Game.Gameplay.Runtime.Story.QuestTree;
using Nomad.Game.Gameplay.Runtime.Story.State;
using Nomad.Game.Sdk.Story;

namespace Nomad.Game.Gameplay.Runtime.Story
{
    internal sealed class CompiledStoryDatabase
    {
        public static readonly CompiledStoryDatabase Empty = new CompiledStoryDatabase(
            CompiledStoryStateDatabaseData.Empty,
            Array.Empty<CompiledQuestTree>(),
            new Dictionary<StorylineDefinitionId, ushort>()
        );

        private readonly CompiledQuestTree[] _questTrees;
        private readonly Dictionary<StorylineDefinitionId, ushort> _storylineIndexById;

        public readonly CompiledStoryStateDatabaseData StateData;

        public int StorylineCount => _questTrees.Length;
        public ReadOnlySpan<CompiledQuestTree> QuestTrees => _questTrees;

        public CompiledStoryDatabase(
            CompiledStoryStateDatabaseData stateData,
            CompiledQuestTree[] questTrees,
            Dictionary<StorylineDefinitionId, ushort> storylineIndexById)
        {
            StateData = stateData ?? CompiledStoryStateDatabaseData.Empty;
            _questTrees = questTrees ?? Array.Empty<CompiledQuestTree>();
            _storylineIndexById = storylineIndexById ?? new Dictionary<StorylineDefinitionId, ushort>();
        }

        public bool TryGetStorylineIndex(StorylineDefinitionId id, out ushort index)
        {
            return _storylineIndexById.TryGetValue(id, out index);
        }

        public bool TryGetQuestTree(StorylineDefinitionId id, out CompiledQuestTree tree)
        {
            if (_storylineIndexById.TryGetValue(id, out ushort index)) {
                tree = _questTrees[index];
                return true;
            }

            tree = null;
            return false;
        }
    }
}
