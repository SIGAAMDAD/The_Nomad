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

namespace Nomad.Game.Sdk.Story.QuestTree
{
    public readonly struct QuestNodeTypeInfo
    {
        public readonly QuestNodeType Type;
        public readonly QuestNodeTraversalFlags Flags;

        public bool CanEnter => (Flags & QuestNodeTraversalFlags.CanEnter) != 0;
        public bool CanExit => (Flags & QuestNodeTraversalFlags.CanExit) != 0;
        public bool IsTerminal => (Flags & QuestNodeTraversalFlags.EndsTree) != 0;
        public bool IsControlNode => (Flags & (QuestNodeTraversalFlags.Branches | QuestNodeTraversalFlags.Joins | QuestNodeTraversalFlags.Gates | QuestNodeTraversalFlags.Waits | QuestNodeTraversalFlags.EmitsEvent | QuestNodeTraversalFlags.Cleanup)) != 0;

        public QuestNodeTypeInfo(QuestNodeType type, QuestNodeTraversalFlags flags)
        {
            Type = type;
            Flags = flags;
        }
    }
}
