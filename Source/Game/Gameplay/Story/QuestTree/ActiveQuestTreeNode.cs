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

using Nomad.Game.Sdk.Story;
using Nomad.Game.Sdk.Story.QuestTree;
using Nomad.Game.Gameplay.Runtime.Story.QuestTree;

namespace Nomad.Game.Gameplay.Story.QuestTree
{
	internal struct ActiveQuestTreeNode
    {
        public QuestDefinitionId QuestId;
        public QuestNodeType Type;
        public QuestNodeTraversalFlags TraversalFlags;
        public bool IsHidden;
        public ushort FirstChild;
        public ushort ChildCount;
        public ulong RequiredCompletedMask;
        public ulong RequiredFailedMask;
        public ulong RequiredTerminalMask;
        public CompiledQuestCondition Condition;
    };
};
