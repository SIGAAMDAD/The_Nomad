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

namespace Nomad.Game.Sdk.Story.QuestTree
{
    [Flags]
    public enum QuestNodeTraversalFlags : ushort
    {
        None = 0,
        CanEnter = 1 << 0,
        CanExit = 1 << 1,
        StartsTree = 1 << 2,
        EndsTree = 1 << 3,
        Branches = 1 << 4,
        Joins = 1 << 5,
        Gates = 1 << 6,
        Waits = 1 << 7,
        EmitsEvent = 1 << 8,
        AdvancesQuestProgress = 1 << 9,
        RequiresQuestCompletionToExit = 1 << 10,
        RequiresQuestFailureToExit = 1 << 11,
        Cleanup = 1 << 12,
        HiddenByDefault = 1 << 13
    }
}
